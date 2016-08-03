namespace Dsp.Web.Areas.Nme.Controllers
{
    using Data.Entities;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Web.Controllers;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class BroQuestController : BaseController
    {
        public async Task<ActionResult> Index(bool i = true, bool c = true)
        {
            ViewBag.i = i;
            ViewBag.c = c;
            Semester semester = await GetThisSemesterAsync();
            var model = new BroQuestIndexModel(semester);
            // Get members list depending on whether or not the current user is an active or new member
            model.Member = await UserManager.FindByNameAsync(User.Identity.Name);
            model.Members = new List<Member>();

            var roster = model.Members = await base.GetRosterForSemester(semester); 
            if (User.IsInRole("Active"))
            {
                model.Members = roster.Where(m => m.MemberStatus.StatusName == "Pledge");
                return View("IndexMember", model);
            }
            else if(User.IsInRole("Pledge"))
            {
                model.Members = roster.Where(m => m.MemberStatus.StatusName == "Active");
                return View("IndexNewMember", model);
            }
            else if (User.IsInRole("Administrator"))
            {
                model.Members = roster;
                return View("IndexMember", model);
            }

            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        [Authorize(Roles = "Administrator, New Member Educator")]
        public ActionResult Manager()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailMessage = TempData["FailureMessage"];

            return View();
        }

        [Authorize(Roles = "Administrator, Active")]
        public async Task<ActionResult> MyChallenges()
        {
            var semester = await GetThisSemesterAsync();
            var member = await UserManager.FindByNameAsync(User.Identity.Name);
            var model = new BroQuestChallengeModel(semester, member);

            return View(model);
        }

        [Authorize(Roles = "Administrator, Pledge")]
        public async Task<ActionResult> Challenges(string userName)
        {
            if(string.IsNullOrEmpty(userName)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            

            var semester = await GetThisSemesterAsync();
            var member = await UserManager.FindByNameAsync(userName);
            var model = new BroQuestChallengeModel(semester, member);

            return View(model);
        }
    }
}