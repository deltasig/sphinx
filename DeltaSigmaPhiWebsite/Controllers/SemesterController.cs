namespace DeltaSigmaPhiWebsite.Controllers
{
    using System.Linq;
    using Data.UnitOfWork;
    using Models;
    using System.Net;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, Secretary")]
    public class SemesterController : BaseController
    {
        public SemesterController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        // GET: Semesters
        public ActionResult Index()
        {
            return View(uow.SemesterRepository.GetAll().OrderByDescending(s => s.DateStart));
        }
        
        // GET: Semesters/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Semesters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SemesterId,DateStart,DateEnd")] Semester semester)
        {
            if (!ModelState.IsValid) return View(semester);

            uow.SemesterRepository.Insert(semester);
            uow.Save();
            return RedirectToAction("Index");
        }

        // GET: Semesters/Edit/5
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

        // POST: Semesters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SemesterId,DateStart,DateEnd")] Semester semester)
        {
            if (!ModelState.IsValid) return View(semester);

            uow.SemesterRepository.Update(semester);
            uow.Save();
            return RedirectToAction("Index");
        }

        // GET: Semesters/Delete/5
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

        // POST: Semesters/Delete/5
        [HttpPost, ActionName("Delete")]
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
