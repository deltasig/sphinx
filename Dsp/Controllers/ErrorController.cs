namespace Dsp.Controllers
{
    using Data;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Xml;

    [Authorize(Roles = "Administrator")]
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return InternalServerError();
        }

        [AllowAnonymous]
        public ActionResult NotFound()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            return View("NotFound");
        }

        [AllowAnonymous]
        public ActionResult InternalServerError()
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return View("InternalServerError");
        }

        public async Task<ActionResult> Logs(int page = 1)
        {
            List<ElmahErrorLog> logs;
            using (var db = new ElmahDbContext())
            {
                const int pageSize = 10;
                var logsCount = await db.Errors.CountAsync();
                var pageCount = logsCount / pageSize;
                // Make sure no improper page values were entered
                page = page < 1 ? 1 : page;
                page = page > pageCount && pageCount > 0 ? pageCount : page;
                // Set ViewBag properties for paging (required for pager to function properly)
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.Count = logsCount;
                ViewBag.Pages = pageCount;

                // Load logs excluding details because the details are big XML blobs
                logs = (await db.Errors
                    .OrderByDescending(e => e.TimeUtc)
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .Select(e => new { e.ErrorId, e.StatusCode, e.User, e.Type, e.Source, e.Message, e.TimeUtc }).ToListAsync())
                    .Select(e => new ElmahErrorLog
                    {
                        ErrorId = e.ErrorId,
                        StatusCode = e.StatusCode,
                        User = e.User,
                        Type = e.Type,
                        Source = e.Source,
                        Message = e.Message,
                        TimeUtc = e.TimeUtc
                    }).ToList();
            }
            return View(logs);
        }

        public async Task<JsonResult> GetLogDetails(Guid id)
        {
            string data;
            using (var db = new ElmahDbContext())
            {
                var log = await db.Errors.FindAsync(id);
                using (var reader = XmlReader.Create(new StringReader(log.AllXml)))
                {
                    reader.ReadToFollowing("error");
                    reader.MoveToAttribute("detail");
                    data = reader.Value;
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}