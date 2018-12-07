namespace Dsp.Web.Areas.Members.Controllers
{
    using Dsp.Data.Entities;
    using Dsp.Web.Controllers;
    using Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Affiliate")]
    public class RosterController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(RosterFilterModel filter)
        {
            Semester semester;
            if (filter.sem == null)
            {
                semester = await base.GetThisSemesterAsync();
                filter.sem = semester.SemesterId;
            }
            else
            {
                semester = await _db.Semesters.FindAsync(filter.sem);
            }

            var members = await base.GetRosterForSemester(semester);
            ViewBag.Sort = filter.sort;
            ViewBag.Order = filter.order;
            ViewBag.SearchTerm = filter.s;
            var filteredResults = base.GetFilteredMembersList(members, filter.s, filter.sort, filter.order);

            var model = new RosterIndexModel
            {
                SelectedSemester = semester.SemesterId,
                Semester = semester,
                Semesters = await base.GetSemesterListAsync(),
                Members = filteredResults
            };

            return View(model);
        }

        [HttpGet, Authorize(Roles = "Administrator, Secretary")]
        public async Task<ActionResult> InitiateNewMembers(string message)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            var model = new InitiateNewMembersModel
            {
                NewMembers = await GetNewMemberUserIdListAsFullNameAsync()
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Secretary")]
        public async Task<ActionResult> InitiateNewMembers(InitiateNewMembersModel model)
        {
            var newMembers = await _db.Users
                .Where(m =>
                    model.SelectedMemberIds.Contains(m.Id))
                .ToListAsync();
            var activeId = (await _db.MemberStatuses.SingleAsync(s => s.StatusName == "Active")).StatusId;

            foreach (var m in newMembers)
            {
                m.StatusId = activeId;
                _db.Entry(m).State = EntityState.Modified;
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "New members successfully moved to active status.";
            return RedirectToAction("InitiatePledges");
        }

        [HttpGet, Authorize(Roles = "Administrator, Secretary")]
        public async Task<ActionResult> GraduateActives(string message)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            var model = new GraduateActivesModel
            {
                Actives = await GetGraduatingActiveUserIdListAsFullNameAsync()
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Secretary")]
        public async Task<ActionResult> GraduateActives(GraduateActivesModel model)
        {
            var actives = await _db.Users
                .Where(m =>
                    model.SelectedMemberIds.Contains(m.Id))
                .ToListAsync();
            var alumnusId = (await _db.MemberStatuses.SingleAsync(s => s.StatusName == "Alumnus")).StatusId;

            foreach (var p in actives)
            {
                p.StatusId = alumnusId;
                _db.Entry(p).State = EntityState.Modified;
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Actives successfully moved to alumni status.";
            return RedirectToAction("GraduateActives");
        }

        [HttpGet]
        public async Task<FileContentResult> Download()
        {
            var members = await UserManager.Users
                .OrderBy(m => m.MemberStatus.StatusId)
                .ThenBy(m => m.LastName)
                .ToListAsync();
            const string header = "First Name, Last Name, Mobile, Email, Member Status, Pledge Class, Pin, Graduation, Location, Big Bro";
            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (var m in members)
            {
                var firstName = m.FirstName;
                var lastName = m.LastName;
                var phone = m.PhoneNumbers.SingleOrDefault(p => p.Type == "Mobile")?.Number ?? "None";
                var email = m.Email;
                var status = m.MemberStatus.StatusName;
                var pledgeClass = m.PledgeClass?.PledgeClassName ?? "None";
                var pin = m.Pin?.ToString() ?? "None";
                var graduationSemester = m.GraduationSemester?.ToString() ?? "None";
                var location = m.RoomString();
                var bigBro = m.BigBrother == null ? "None" : m.BigBrother.FirstName + " " + m.BigBrother.LastName;
                var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    firstName,
                    lastName,
                    phone,
                    email,
                    status,
                    pledgeClass,
                    pin,
                    graduationSemester,
                    location,
                    bigBro);
                sb.AppendLine(line);
            }

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "dsp-roster.csv");
        }
    }
}
