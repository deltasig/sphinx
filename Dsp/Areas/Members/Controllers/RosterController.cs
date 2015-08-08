namespace Dsp.Areas.Members.Controllers
{
    using Dsp.Controllers;
    using Entities;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class RosterController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(int? sid)
        {
            Semester semester;
            if (sid == null)
            {
                semester = await base.GetThisSemesterAsync();
            }
            else
            {
                semester = await _db.Semesters.FindAsync(sid);
            }

            var model = new RosterIndexModel
            {
                SelectedSemester = semester.SemesterId,
                Semester = semester,
                Semesters = await base.GetSemesterListAsync(),
                Members = await base.GetRosterForSemester(semester)
            };

            return View(model);
        }


        [HttpGet, Authorize(Roles = "Administrator, Secretary")]
        public async Task<ActionResult> InitiatePledges(string message)
        {
            ViewBag.SuccessMessage = message;

            var model = new InitiatePledgesModel
            {
                Pledges = await GetPledgeUserIdListAsFullNameAsync()
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Secretary")]
        public async Task<ActionResult> InitiatePledges(InitiatePledgesModel model)
        {
            var pledges = await UserManager.Users
                .Where(m => 
                    model.SelectedMemberIds.Contains(m.Id))
                .ToListAsync();
            var activeId = (await _db.MemberStatus.SingleAsync(s => s.StatusName == "Active")).StatusId;

            foreach(var p in pledges)
            {
                p.StatusId = activeId;
                _db.Entry(p).State = EntityState.Modified;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("InitiatePledges", new { message = "Pledges successfully moved to active status." });
        }
        
        [HttpGet]
        public async Task<FileContentResult> Download()
        {
            var members = await UserManager.Users
                .OrderBy(m => m.MemberStatus.StatusId)
                .ThenBy(m => m.LastName)
                .ToListAsync();
            const string header = "First Name, Last Name, Mobile, Email, Member Status, Pledge Class, Pin, Graduation, Room, Big Bro, T-Shirt";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (var m in members)
            {
                var phone = m.PhoneNumbers.SingleOrDefault(p => p.Type == "Mobile");
                var line = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    m.FirstName,
                    m.LastName,
                    phone == null ? "None" : phone.Number,
                    m.Email,
                    m.MemberStatus.StatusName,
                    m.PledgeClass.PledgeClassName,
                    m.Pin,
                    m.GraduationSemester.ToString(),
                    m.RoomString(),
                    m.BigBrother == null ? "None" : m.BigBrother.FirstName + " " + m.BigBrother.LastName,
                    m.ShirtSize);
                sb.AppendLine(line);
            }

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "dsp-roster.csv");
        }
    }
}
