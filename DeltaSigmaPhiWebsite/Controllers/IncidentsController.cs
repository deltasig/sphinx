namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using System;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class IncidentsController : BaseController
    {
        public IncidentsController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        public ActionResult Index()
        {
            var model = uow.IncidentReportRepository.SelectAll().ToList();
            return View(model);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var incidentReport = uow.IncidentReportRepository.SingleById(id);
            if (incidentReport == null)
            {
                return HttpNotFound();
            }
            return View(incidentReport);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IncidentId,DateTimeSubmitted,ReportedBy,DateTimeOfIncident,PolicyBroken,Description,OfficialReport")] IncidentReport incidentReport)
        {
            if (!ModelState.IsValid) return View(incidentReport);
            incidentReport.DateTimeSubmitted = DateTime.UtcNow;
            incidentReport.ReportedBy = uow.MemberRepository.Single(m => m.UserName == User.Identity.Name).UserId;
            
            uow.IncidentReportRepository.Insert(incidentReport);
            uow.Save();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var incidentReport = uow.IncidentReportRepository.SingleById(id);
            if (incidentReport == null)
            {
                return HttpNotFound();
            }

            return View(incidentReport);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public ActionResult Edit([Bind(Include = "IncidentId,DateTimeSubmitted,ReportedBy,DateTimeOfIncident,PolicyBroken,Description,OfficialReport")] IncidentReport incidentReport)
        {
            if (!ModelState.IsValid) return View(incidentReport);
            uow.IncidentReportRepository.Update(incidentReport);
            uow.Save();
            return RedirectToAction("Index");
        }
    }
}
