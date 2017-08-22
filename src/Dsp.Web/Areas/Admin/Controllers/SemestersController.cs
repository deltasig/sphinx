namespace Dsp.Web.Areas.Admin.Controllers
{
    using Dsp.Web.Controllers;
    using Dsp.Data.Entities;
    using Models;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Text;

    [Authorize(Roles = "Administrator, President, Secretary, Academics, Service")]
    public class SemestersController : BaseController
    {
        private readonly static List<string> _greekAlphabet = new List<string>
        {
            "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta",
            "Theta", "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron",
            "Pi", "Rho", "Sigma", "Tau", "Upsilon", "Phi", "Chi", "Psi", "Omega"
        };

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            return View(await _db.Semesters.OrderByDescending(s => s.DateStart).ToListAsync());
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var latestSemester = (await _db.Semesters.OrderBy(s => s.DateStart).ToListAsync()).LastOrDefault();
            var latestPledgeClass = latestSemester.PledgeClasses.FirstOrDefault();
            
            var model = new CreateSemesterModel
            {
                GreekAlphabet = _greekAlphabet,
                Semester = new Semester(),
                PledgeClass = new PledgeClass()
            };

            // Try to fill in some of the data for them based on existing information.
            if (latestSemester != null)
            {
                if (latestSemester.DateStart.Month < 5)
                {
                    model.Semester.DateStart = latestSemester.DateStart.AddMonths(7);
                    model.Semester.DateEnd = latestSemester.DateEnd.AddMonths(7);
                    model.Semester.TransitionDate = latestSemester.TransitionDate.AddMonths(7);
                    if (latestPledgeClass != null && latestPledgeClass.InitiationDate != null)
                    {
                        model.PledgeClass.InitiationDate = ((DateTime)latestPledgeClass.InitiationDate).AddMonths(7);
                    }
                    if (latestPledgeClass != null && latestPledgeClass.PinningDate != null)
                    {
                        model.PledgeClass.PinningDate = ((DateTime)latestPledgeClass.PinningDate).AddMonths(7);
                    }
                }
                else
                {
                    model.Semester.DateStart = latestSemester.DateStart.AddMonths(5);
                    model.Semester.DateEnd = latestSemester.DateEnd.AddMonths(5);
                    model.Semester.TransitionDate = latestSemester.TransitionDate.AddMonths(5);
                    if (latestPledgeClass != null && latestPledgeClass.InitiationDate != null)
                    {
                        model.PledgeClass.InitiationDate = ((DateTime)latestPledgeClass.InitiationDate).AddMonths(5);
                    }
                    if (latestPledgeClass != null && latestPledgeClass.PinningDate != null)
                    {
                        model.PledgeClass.PinningDate = ((DateTime)latestPledgeClass.PinningDate).AddMonths(5);
                    }
                }

                model.Semester.MinimumServiceEvents = latestSemester.MinimumServiceEvents;
                model.Semester.MinimumServiceHours = latestSemester.MinimumServiceHours;

                // Guess at next pledge class name...
                if (latestPledgeClass != null)
                {
                    var nameParts = latestPledgeClass.PledgeClassName.Split(' ');
                    var nameIndeces = new List<int>();
                    foreach(var p in nameParts)
                    {
                        var alphabetPosition = _greekAlphabet.IndexOf(p);
                        if(alphabetPosition >= 0)
                        {
                            nameIndeces.Add(alphabetPosition);
                        }
                    }
                    var sb = new StringBuilder();
                    for(var i = 0; i < nameIndeces.Count; i++)
                    {
                        var index = nameIndeces[i];
                        if(i + 1 >= nameIndeces.Count)
                        {
                            if (index >= _greekAlphabet.Count)
                            {
                                index = 0;
                            }
                            else
                            {
                                index++;
                            }
                        }
                        var letter = _greekAlphabet[index];
                        sb.Append(_greekAlphabet[index]);
                        sb.Append(" ");
                    }
                    model.PledgeClass.PledgeClassName = sb.ToString().TrimEnd();
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateSemesterModel model)
        {
            if (!ModelState.IsValid) return View(model);
            // Validate Pledge Class Name
            model.PledgeClass.PledgeClassName = base.ToTitleCaseString(model.PledgeClass.PledgeClassName);
            var nameParts = model.PledgeClass.PledgeClassName.Split(' ');
            foreach(var n in nameParts)
            {
                if (!_greekAlphabet.Contains(n))
                {
                    TempData["FailureMessage"] = "The pledge class name you entered is not valid. Please verify the spelling of each part and try again.";
                    return View(model);
                }
            }

            // Add the semester first because we need its Id for the pledge class.
            model.Semester.DateStart = base.ConvertCstToUtc(model.Semester.DateStart);
            model.Semester.DateEnd = base.ConvertCstToUtc(model.Semester.DateEnd);
            model.Semester.TransitionDate = base.ConvertCstToUtc(model.Semester.TransitionDate);
            _db.Semesters.Add(model.Semester);
            await _db.SaveChangesAsync();

            // Add the pledge class to the newly added semester.
            if (model.PledgeClass.InitiationDate != null)
            {
                model.PledgeClass.InitiationDate = base.ConvertCstToUtc((DateTime)model.PledgeClass.InitiationDate);
            }
            if (model.PledgeClass.PinningDate != null)
            {
                model.PledgeClass.PinningDate = base.ConvertCstToUtc((DateTime)model.PledgeClass.PinningDate);
            }
            model.PledgeClass.SemesterId = model.Semester.SemesterId;
            _db.PledgeClasses.Add(model.PledgeClass);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{model.Semester.ToString()} semester added!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = await _db.Semesters.FindAsync(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            semester.DateStart = base.ConvertUtcToCst(semester.DateStart);
            semester.DateEnd = base.ConvertUtcToCst(semester.DateEnd);
            semester.TransitionDate = base.ConvertUtcToCst(semester.TransitionDate);
            return View(semester);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Semester semester)
        {
            if (!ModelState.IsValid) return View(semester);

            semester.DateStart = base.ConvertCstToUtc(semester.DateStart);
            semester.DateEnd = base.ConvertCstToUtc(semester.DateEnd);
            semester.TransitionDate = base.ConvertCstToUtc(semester.TransitionDate);
            _db.Entry(semester).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = await _db.Semesters.FindAsync(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            if(semester.ClassesTaken.Any() || semester.GraduatingMembers.Any() || semester.Leaders.Any() ||
               semester.Rooms.Any() || semester.ServiceEventAmendments.Any() || semester.ServiceHourAmendments.Any())
            {
                TempData["FailureMessage"] = "Can't delete this semester because related data exists and would also be deleted.";
                return RedirectToAction("Index");
            }
            return View(semester);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var semester = await _db.Semesters.FindAsync(id);
            _db.Semesters.Remove(semester);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
