namespace Dsp.Web.Areas.Scholarships.Controllers
{
    using Dsp.Web.Controllers;
    using Dsp.Data.Entities;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
    public class QuestionsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.SuccessMessage = TempData[SuccessMessageKey];
            ViewBag.FailureMessage = TempData[FailureMessageKey];
            return View(await _db.ScholarshipQuestions.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var scholarshipQuestion = await _db.ScholarshipQuestions.FindAsync(id);
            if (scholarshipQuestion == null) return HttpNotFound();

            return View(scholarshipQuestion);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ScholarshipQuestion model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.ScholarshipQuestions.Add(model);
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Question successfully added to pool.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.ScholarshipQuestions.FindAsync(id);
            if (model == null) return HttpNotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ScholarshipQuestion model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Question successfully modified.";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var scholarshipQuestion = await _db.ScholarshipQuestions.FindAsync(id);
            if (scholarshipQuestion == null) return HttpNotFound(); 

            return View(scholarshipQuestion);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var scholarshipQuestion = await _db.ScholarshipQuestions.FindAsync(id);
            if (scholarshipQuestion.Answers.Any())
            {
                TempData[FailureMessageKey] = 
                    "Scholarship Question could not be deleted because it has existing answers associated with it.";
                return RedirectToAction("Index");
            }
            _db.ScholarshipQuestions.Remove(scholarshipQuestion);
            await _db.SaveChangesAsync();

            TempData[SuccessMessageKey] = "Question successfully deleted.";
            return RedirectToAction("Index");
        }
    }
}
