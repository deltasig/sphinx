namespace DeltaSigmaPhiWebsite.Areas.Members.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using System;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class IncidentsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var model = await _db.IncidentReports.ToListAsync();
            return View(model);
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var incidentReport = await _db.IncidentReports.FindAsync(id);
            if (incidentReport == null)
            {
                return HttpNotFound();
            }
            return View(incidentReport);
        }

        public ActionResult Submit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(IncidentReport incidentReport)
        {
            if (!ModelState.IsValid) return View(incidentReport);
            incidentReport.DateTimeSubmitted = DateTime.UtcNow;
            incidentReport.ReportedBy = (await _db.Members.SingleAsync(m => m.UserName == User.Identity.Name)).UserId;
            
            _db.IncidentReports.Add(incidentReport);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var incidentReport = await _db.IncidentReports.FindAsync(id);
            if (incidentReport == null)
            {
                return HttpNotFound();
            }

            return View(incidentReport);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Sergeant-at-Arms")]
        public async Task<ActionResult> Edit(IncidentReport incidentReport)
        {
            if (!ModelState.IsValid) return View(incidentReport);

            _db.Entry(incidentReport).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
