namespace Dsp.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using Elmah;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using System.Web.Mvc;
    using Data;

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

        public async Task<ActionResult> Logs()
        {
            List<ElmahErrorLog> errors;
            
            using (var db = new ElmahDbContext())
            {
                errors = await db.Errors.ToListAsync();
            }

            return View(errors);
        }
    }
}