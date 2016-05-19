namespace Dsp.Web.Controllers
{
    using Dsp.Data;
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
        public ActionResult Index()
        {
            return HttpNotFound();
        }

        public async Task<ActionResult> Logs(int page = 1)
        {
            List<ElmahErrorLog> logs;
            using (var db = new ElmahDbContext())
            {
                const int pageSize = 10;
                var logsCount = await db.Errors.CountAsync();
                // Set ViewBag properties for paging (required for pager to function properly)
                if (page < 1) page = 1;
                ViewBag.Count = logsCount;
                ViewBag.PageSize = pageSize;
                ViewBag.Pages = logsCount / pageSize;
                ViewBag.Page = page;
                if (logsCount % pageSize != 0) ViewBag.Pages += 1;
                if (page > ViewBag.Pages) ViewBag.Page = ViewBag.Pages;

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