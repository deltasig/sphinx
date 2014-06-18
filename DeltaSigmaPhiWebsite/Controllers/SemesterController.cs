namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using Models.ViewModels;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    [Authorize(Roles = "Active, Administrator")]
    public class SemesterController : BaseController
    {
        public SemesterController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        [HttpGet]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        public ActionResult Index()
        {
            return View(uow.SemesterRepository.GetAll().OrderByDescending(s => s.DateStart).ToList());
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SemesterId,DateStart,DateEnd")] CreateSemesterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            uow.SemesterRepository.Insert(model.Semester);
            uow.Save();
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = uow.SemesterRepository.GetById(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, President, Secretary, Academics")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SemesterId,DateStart,DateEnd")] Semester semester)
        {
            if (!ModelState.IsValid) return View(semester);

            uow.SemesterRepository.Update(semester);
            uow.Save();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = uow.SemesterRepository.GetById(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            return View(semester);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var semester = uow.SemesterRepository.GetById(id);
            uow.SemesterRepository.Delete(semester);
            uow.Save();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                uow.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
