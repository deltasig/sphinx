using Dsp.Data;
using Dsp.Data.Entities;
using Dsp.Services.Admin;
using Dsp.Services.Interfaces;
using Dsp.Web.Extensions;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Dsp.Web.Api
{
    [Authorize(Roles = "Active, Alumnus, Pledge")]
    [RoutePrefix("api/broquest")]
    public class BroQuestController : ApiController
    {
        private SphinxDbContext _db;
        private ISemesterService _semesterService;
        private IMemberService _memberService;
        private IBroQuestService _broQuestService;
        public BroQuestController()
        {
            _db = new SphinxDbContext();
            _semesterService = new SemesterService(_db);
            _memberService = new MemberService(_db);
            _broQuestService = new BroQuestService(_db);
        }

        [Route("period")]
        public async Task<IHttpActionResult> GetPeriod()
        {
            var semester = await _semesterService.GetCurrentSemesterAsync();
            if (semester == null) return NotFound();
            var beginsOn = semester.QuestingBeginsOn.FromUtcToCst();
            var endsOn = semester.QuestingEndsOn.FromUtcToCst();
            return Ok(new { BeginsOn = beginsOn, EndsOn = endsOn });
        }

        [Route("period/{id:int}")]
        public async Task<IHttpActionResult> GetPeriod(int id)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(id);
            if (semester == null) return NotFound();
            var beginsOn = semester.QuestingBeginsOn.FromUtcToCst();
            var endsOn = semester.QuestingEndsOn.FromUtcToCst();
            return Ok(new { BeginsOn = semester.QuestingBeginsOn, EndsOn = semester.QuestingEndsOn });
        }

        [Route("timeleft")]
        public async Task<IHttpActionResult> GetTimeLeftBegins()
        {
            var semester = await _semesterService.GetCurrentSemesterAsync();
            if (semester == null) return NotFound();
            var now = DateTime.UtcNow;
            var beginsOn = semester.QuestingBeginsOn;
            var endsOn = semester.QuestingEndsOn;
            var timeLeft = string.Empty;
            if (now < beginsOn) // Questing hasn't started
            {
                timeLeft = (beginsOn - now).ToPreciseTimeUntilString();
                timeLeft = string.Format("Questing begins in: {0}", timeLeft);
            }
            else if (now < endsOn) // Questing has started, but not finished
            {
                timeLeft = (endsOn - now).ToPreciseTimeUntilString();
                timeLeft = string.Format("Questing ends in: {0}", timeLeft);
            }
            else // Questing is over and we're waiting on the next semester
            {
                timeLeft = "Questing is over.  Check back next semester.";
            }

            return Ok(timeLeft);
        }

        [Route("progress/{id:int}")]
        public async Task<IHttpActionResult> GetMemberProgress(int id)
        {
            var semester = await _semesterService.GetCurrentSemesterAsync();
            if (semester == null) return NotFound();
            var member = _db.Users.Find(id);
            if (member == null) return NotFound();
            var status = member.MemberStatus.StatusName;
            var progress = 0;
            var pledges = await _memberService.GetNewMembersAsync(semester);
            var actives = await _memberService.GetActivesAsync(semester);
            var activeCount = actives.Count();

            if (status == "Active")
            {
                if (pledges.Any())
                {
                    var thisUsersCompletions = member.QuestChallenges.Where(c => c.SemesterId == semester.SemesterId);
                    var completed = thisUsersCompletions.Where(c => c.Completions.Any());
                    var percent = (completed.Count() / pledges.Count()) * 100;
                    progress = percent > 100 ? 100 : percent;
                }
            }
            else if (status == "Pledge")
            {
                if (actives.Any())
                {
                    var activesCompleted = member
                        .QuestCompletions.Where(c =>
                            c.Challenge.SemesterId == semester.SemesterId &&
                            c.Challenge.EndsOn < DateTime.UtcNow.FromUtcToCst());
                    progress = (100 * activesCompleted.Count()) / actives.Count();
                }
            }

            return Ok(progress);
        }

        [Route("challenges/{mid:int}/{sid:int}")]
        public async Task<IHttpActionResult> GetMemberChallenges(int mid, int sid)
        {
            var semester = await _semesterService.GetCurrentSemesterAsync();
            if (semester == null) return NotFound();
            var member = _db.Users.Find(mid);
            if (member == null) return NotFound();

            var slots = new List<Slot>();
            var challenges = await _broQuestService.GetChallengesForMemberAsync(mid, sid);
            foreach (var c in challenges)
            {
                var start = c.BeginsOn;
                var end = c.EndsOn;
                TimeSpan elapsedSpan = new TimeSpan(start.Ticks);
                var stop = elapsedSpan.TotalMinutes + (end - start).TotalMinutes;
                for (var i = elapsedSpan.TotalMinutes; i < stop; i += 15)
                {
                    var slot = new Slot();
                    slot.time = (int)i;
                    slot.newMembers = new List<Tuple<int, string>>();
                    foreach (var comp in c.Completions)
                    {
                        var tup = new Tuple<int, string>(comp.NewMemberId, comp.Member.ToString());
                        slot.newMembers.Add(tup);
                    }
                    if (c.Completions.Count >= member.MaxQuesters) slot.isFull = true;
                    slots.Add(slot);
                }
            }

            return Ok(slots);
        }

        [Route("challenge")]
        [HttpPost, ResponseType(typeof(NewChallenge))]
        public async Task<IHttpActionResult> AddChallenge([FromBody] NewChallenge body)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(body.sid);
            if (semester == null) return NotFound();
            if (body.mid.ToString() != User.Identity.GetUserId()) return NotFound();
            var member = _db.Users.Find(body.mid);
            if (member == null) return NotFound();

            var start = DateTime.MinValue.AddMinutes(body.mins);
            var end = start.AddMinutes(member.QuestChallengeSize);

            if (start < DateTime.UtcNow.FromUtcToCst()) return Ok("This challenge would go outside the questing period.");
            if (end > semester.QuestingEndsOn.FromUtcToCst() || start < semester.QuestingBeginsOn.FromUtcToCst())
                return Ok("This challenge would go outside the questing period.");

            var conflict = await _db.QuestChallenges
                .AnyAsync(q =>
                    (q.BeginsOn < start && start < q.EndsOn) ||
                    (q.BeginsOn < end && end < q.EndsOn) ||
                    (q.BeginsOn < start && end < q.EndsOn));
            if (conflict) return Ok("This challenge overlaps with an existing one.");

            var challenge = new QuestChallenge();
            challenge.BeginsOn = start;
            challenge.EndsOn = end;
            challenge.MemberId = member.Id;
            challenge.SemesterId = semester.SemesterId;

            try
            {
                await _broQuestService.AddChallengeAsync(member.Id, semester.SemesterId, start, end);

                var slots = new List<int>();
                TimeSpan elapsedSpan = new TimeSpan(start.Ticks);
                var stop = elapsedSpan.TotalMinutes + (end - start).TotalMinutes;
                for (var i = elapsedSpan.TotalMinutes; i < stop; i += 15)
                {
                    slots.Add((int)i);
                }
                return Ok(slots);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Route("challenge")]
        [HttpDelete, ResponseType(typeof(NewChallenge))]
        public async Task<IHttpActionResult> DeleteChallenge([FromBody] NewChallenge body)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(body.sid);
            if (semester == null) return NotFound();
            if (body.mid.ToString() != User.Identity.GetUserId()) return NotFound();
            var member = _db.Users.Find(body.mid);
            if (member == null) return NotFound();

            var now = DateTime.UtcNow.FromUtcToCst();
            var start = DateTime.MinValue.AddMinutes(body.mins);
            var end = start.AddMinutes(15);

            var challenge = await _db.QuestChallenges
                .SingleOrDefaultAsync(c =>
                    c.MemberId == member.Id &&
                    c.SemesterId == semester.SemesterId &&
                    c.BeginsOn <= start && end <= c.EndsOn);
            if (challenge == null) return Ok("The challenge requested to be deleted does not exist.");
            if (challenge.BeginsOn < now)
                return Ok("The challenge requested to be deleted can't be deleted because it's already started/occurred.");

            try
            {
                start = challenge.BeginsOn;
                end = challenge.EndsOn;
                var slots = new List<int>();
                TimeSpan elapsedSpan = new TimeSpan(start.Ticks);
                var stop = elapsedSpan.TotalMinutes + (end - start).TotalMinutes;
                for (var i = elapsedSpan.TotalMinutes; i < stop; i += 15)
                {
                    slots.Add((int)i);
                }

                await _broQuestService.DeleteChallengeAsync(challenge);

                return Ok(slots);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Route("complete")]
        [HttpPost, ResponseType(typeof(NewQuickCompletion))]
        public async Task<IHttpActionResult> MarkComplete([FromBody] NewQuickCompletion body)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(body.sid);
            if (semester == null) return NotFound();
            if (body.mid.ToString() != User.Identity.GetUserId()) return NotFound();
            var member = _db.Users.Find(body.mid);
            if (member == null) return NotFound();
            var newMember = _db.Users.Find(body.nmid);
            if (newMember == null) return NotFound();

            try
            {
                var challenge = new QuestChallenge();
                var now = DateTime.UtcNow.FromUtcToCst();
                challenge.BeginsOn = now.AddMinutes(-member.QuestChallengeSize);
                challenge.EndsOn = now;
                challenge.MemberId = member.Id;
                challenge.SemesterId = semester.SemesterId;
                await _broQuestService.AddChallengeAsync(challenge);

                var completion = new QuestCompletion();
                completion.NewMemberId = newMember.Id;
                completion.ChallengeId = challenge.Id;
                completion.IsVerified = true;
                await _broQuestService.AcceptChallengeAsync(completion);

                return Ok();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        
        [Route("complete")]
        [HttpPut, ResponseType(typeof(NewCompletion))]
        public async Task<IHttpActionResult> AcceptChallenge([FromBody] NewCompletion body)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(body.sid);
            if (semester == null) return NotFound();
            if (body.nmid != User.Identity.GetUserId<int>()) return NotFound();
            var member = _db.Users.Find(body.mid);
            if (member == null) return NotFound();

            var now = DateTime.UtcNow.FromUtcToCst();
            var start = DateTime.MinValue.AddMinutes(body.mins);
            var end = start.AddMinutes(15);

            var challenge = await _db.QuestChallenges
                .SingleOrDefaultAsync(c =>
                    c.MemberId == member.Id &&
                    c.SemesterId == semester.SemesterId &&
                    c.BeginsOn <= start && end <= c.EndsOn);
            if (challenge == null) return Ok("You can't back out of this challenge because it doesn't exist.");

            try
            {
                await _broQuestService.AcceptChallengeAsync(challenge.Id, body.nmid, true);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Route("complete")]
        [HttpDelete, ResponseType(typeof(NewCompletion))]
        public async Task<IHttpActionResult> UnacceptChallenge([FromBody] NewCompletion body)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(body.sid);
            if (semester == null) return NotFound();
            if (body.nmid != User.Identity.GetUserId<int>()) return NotFound();
            var member = _db.Users.Find(body.mid);
            if (member == null) return NotFound();

            var now = DateTime.UtcNow.FromUtcToCst();
            var start = DateTime.MinValue.AddMinutes(body.mins);
            var end = start.AddMinutes(15);

            var challenge = await _db.QuestChallenges
                .SingleOrDefaultAsync(c =>
                    c.MemberId == member.Id &&
                    c.SemesterId == semester.SemesterId &&
                    c.BeginsOn <= start && end <= c.EndsOn);
            if (challenge == null) return Ok("You can't back out of this challenge because it doesn't exist.");
            var completion = challenge.Completions.SingleOrDefault(c => c.NewMemberId == body.nmid);
            
            try
            {
                await _broQuestService.UnacceptChallengeAsync(completion);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public class Slot
        {
            public int time { get; set; }
            public bool isFull { get; set; }
            public List<Tuple<int, string>> newMembers { get; set; }
        }

        public class NewChallenge
        {
            public int mid { get; set; }
            public int sid { get; set; }
            public int mins { get; set; }
        }

        public class NewQuickCompletion
        {
            public int mid { get; set; }
            public int nmid { get; set; }
            public int sid { get; set; }
        }

        public class NewCompletion
        {
            public int mins { get; set; }
            public int mid { get; set; }
            public int nmid { get; set; }
            public int sid { get; set; }
        }
    }
}