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
        public async Task<ActionResult> Index(int? s, bool i = true, bool c = false)
        {
            var thisSemester = await GetThisSemesterAsync();
            Semester semester = thisSemester;
            if (s != null)
            {
                semester = await _db.Semesters.FindAsync(s);
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
            // Get members list depending on whether or not the current user is an active or new member
            model.Member = await UserManager.FindByNameAsync(User.Identity.Name);
            model.Members = new List<Member>();
            var roster = model.Members = await base.GetRosterForSemester(semester); 
            if (User.IsInRole("Administrator") || User.IsInRole("New Member Educator"))
            {
                model.Members = roster;
            }
            else if (User.IsInRole("Active"))
            {
                model.Members = roster.Where(m => m.MemberStatus.StatusName == "Pledge");
            }
            else if(User.IsInRole("Pledge"))
            {
                model.Members = roster.Where(m => m.MemberStatus.StatusName == "Active");
            }
            else
            {
                // Nothing for now.
            }

            ViewBag.s = semester.SemesterId;
            ViewBag.i = i;
            ViewBag.c = c;

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