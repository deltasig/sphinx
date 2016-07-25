namespace Dsp.Web.Areas.Nme.Controllers
{
    using Data.Entities;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Web.Controllers;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class BroQuestController : BaseController
    {
        public async Task<ActionResult> Index(int? sid)
        {
            var thisSemester = await GetThisSemesterAsync();
            Semester semester = thisSemester;
            if (sid != null)
            {
                semester = await _db.Semesters.FindAsync(sid);
            }

            var model = new BroQuestIndexModel(semester);

            // Identify valid semesters for dropdown
            var allSemesters = (await _db.Semesters
                .ToListAsync())
                .Except(new List<Semester> { thisSemester });
            var semesters = new List<Semester> { thisSemester };
            foreach (var sem in allSemesters)
            {
                if (sem.QuestChallenges.Any())
                {
                    semesters.Add(sem);
                }
            }
            model.SemesterList = GetCustomSemesterListAsync(semesters);

            return View(model);
        }

        [Authorize(Roles = "Administrator, New Member Educator")]
        public ActionResult Manager()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailMessage = TempData["FailureMessage"];

            return View();
        }
    }
}