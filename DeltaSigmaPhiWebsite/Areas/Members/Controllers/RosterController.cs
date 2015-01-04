namespace DeltaSigmaPhiWebsite.Areas.Members.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class RosterController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(RosterIndexModel model)
        {
            if (model.SearchModel != null)
            {
                if (model.SearchModel.CustomSearchRequested())
                {
                    IEnumerable<Member> guidedSearchResults = await _db.Members
                        .OrderBy(o => o.LastName)
                        .ToListAsync();
                    if (model.SearchModel.SelectedStatusId != -1)
                    {
                        guidedSearchResults = guidedSearchResults.Where(m => m.StatusId == model.SearchModel.SelectedStatusId);
                    }
                    if (model.SearchModel.SelectedPledgeClassId != -1)
                    {
                        guidedSearchResults = guidedSearchResults.Where(m => m.PledgeClassId == model.SearchModel.SelectedPledgeClassId);
                    }
                    if (model.SearchModel.SelectedGraduationSemesterId != -1)
                    {
                        guidedSearchResults = guidedSearchResults.Where(m => m.ExpectedGraduationId == model.SearchModel.SelectedGraduationSemesterId);
                    }
                    switch (model.SearchModel.LivingType)
                    {
                        case "InHouse":
                            guidedSearchResults = guidedSearchResults.Where(m => m.Room != 0);
                            break;
                        case "OutOfHouse":
                            guidedSearchResults = guidedSearchResults.Where(m => m.Room == 0);
                            break;
                    }

                    model.Members = guidedSearchResults.ToList();
                }
                else
                {
                    model.Members = await _db.Members
                        .Where(m => 
                            m.MemberStatus.StatusName == "Pledge" ||
                            m.MemberStatus.StatusName == "Neophyte" ||
                            m.MemberStatus.StatusName == "Active")
                        .OrderBy(o => o.PledgeClassId)
                        .ThenBy(o => o.LastName)
                        .ToListAsync();
                }
            }
            else
            {
                model = new RosterIndexModel
                {
                    Members = await _db.Members
                        .Where(m =>
                            m.MemberStatus.StatusName == "Pledge" ||
                            m.MemberStatus.StatusName == "Neophyte" ||
                            m.MemberStatus.StatusName == "Active")
                        .OrderBy(o => o.PledgeClassId)
                        .ThenBy(o => o.LastName)
                        .ToListAsync()
                };
            }

            model.SearchModel = new RosterIndexSearchModel
            {
                Statuses = await GetStatusListWithNoneAsync(),
                PledgeClasses = await GetPledgeClassListWithNoneAsync(),
                Semesters = await GetSemesterListWithNoneAsync(),
                LivingType = "Both"
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
            var pledges = await _db.Members
                .Where(m => 
                    model.SelectedMemberIds.Contains(m.UserId))
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
            var members = await _db.Members
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
                    m.Semester.ToString(),
                    m.Room.ToString(),
                    m.BigBrother == null ? "None" : m.BigBrother.FirstName + " " + m.BigBrother.LastName,
                    m.ShirtSize);
                sb.AppendLine(line);
            }

            return File(new UTF8Encoding().GetBytes(sb.ToString()), "text/csv", "dsp-roster.csv");
        }
    }
}
