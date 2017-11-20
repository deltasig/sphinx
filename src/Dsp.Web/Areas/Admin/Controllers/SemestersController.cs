namespace Dsp.Web.Areas.Admin.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.Web.Controllers;
    using Models;
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, President, Secretary, Academics, Service")]
    public class SemestersController : BaseController
    {
        private ISemesterService _semesterService;

        public SemestersController()
        {
            _semesterService = new SemesterService(new Repository<SphinxDbContext>(_db));
        }

        public SemestersController(ISemesterService semesterService)
        {
            _semesterService = semesterService;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            var model = await _semesterService.GetAllSemestersAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var latestSemester = await _semesterService.GetFutureMostSemesterAsync();

            var model = new CreateSemesterModel
            {
                GreekAlphabet = _semesterService.Alphabet,
                Semester = _semesterService.GetEstimatedNextSemester(latestSemester),
                PledgeClass = _semesterService.GetEstimatedNextPledgeClass(latestSemester)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateSemesterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Validate Pledge Class Name
                model.PledgeClass.PledgeClassName = base.ToTitleCaseString(model.PledgeClass.PledgeClassName);
                var nameParts = model.PledgeClass.PledgeClassName.Split(' ');
                foreach (var n in nameParts)
                {
                    if (!_semesterService.Alphabet.Contains(n))
                    {
                        TempData["FailureMessage"] = "The pledge class name you entered is not valid. " +
                            "Please verify the spelling of each part and try again.";
                        return View(model);
                    }
                }

                // Add the semester first because we need its Id for the pledge class.
                model.Semester.DateStart = base.ConvertCstToUtc(model.Semester.DateStart);
                model.Semester.DateEnd = base.ConvertCstToUtc(model.Semester.DateEnd);
                model.Semester.TransitionDate = base.ConvertCstToUtc(model.Semester.TransitionDate);
                await _semesterService.AddSemesterAsync(model.Semester);
            }
            catch
            {
                TempData["FailureMessage"] = "Something went wrong when trying to add the semester. " +
                    "Contact your administrator.";
                return View(model);
            }
            try
            {
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
                await _semesterService.AddPledgeClassAsync(model.PledgeClass);
            }
            catch
            {
                TempData["FailureMessage"] = "The semester was added but something went wrong when trying to add the semester. " +
                    "Contact your administrator.";
                return View(model);
            }

            TempData["SuccessMessage"] = $"{model.Semester.ToString()} semester added!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = await _semesterService.GetSemesterByIdAsync(id);
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

            await _semesterService.UpdateSemesterAsync(semester);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var semester = await _semesterService.GetSemesterByIdAsync(id);
            if (semester == null)
            {
                return HttpNotFound();
            }
            if (semester.ClassesTaken.Any() ||
                semester.GraduatingMembers.Any() ||
                semester.Leaders.Any() ||
                semester.Rooms.Any() ||
                semester.ServiceEventAmendments.Any() ||
                semester.ServiceHourAmendments.Any())
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
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            await _semesterService.DeleteSemesterAsync(id);
            return RedirectToAction("Index");
        }
    }
}
