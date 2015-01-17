namespace DeltaSigmaPhiWebsite.Areas.Scholarships.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using System.Data.Entity;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
    public class QuestionsController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            return View(await _db.ScholarshipQuestions.ToListAsync());
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var scholarshipQuestion = await _db.ScholarshipQuestions.FindAsync(id);
            if (scholarshipQuestion == null)
            {
                return HttpNotFound();
            }
            return View(scholarshipQuestion);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ScholarshipQuestion scholarshipQuestion)
        {
            if (!ModelState.IsValid) return View(scholarshipQuestion);

            _db.ScholarshipQuestions.Add(scholarshipQuestion);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var scholarshipQuestion = await _db.ScholarshipQuestions.FindAsync(id);
            if (scholarshipQuestion == null)
            {
                return HttpNotFound();
            }
            return View(scholarshipQuestion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ScholarshipQuestion scholarshipQuestion)
        {
            if (!ModelState.IsValid) return View(scholarshipQuestion);

            _db.Entry(scholarshipQuestion).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var scholarshipQuestion = await _db.ScholarshipQuestions.FindAsync(id);
            if (scholarshipQuestion == null)
            {
                return HttpNotFound();
            }
            return View(scholarshipQuestion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var scholarshipQuestion = await _db.ScholarshipQuestions.FindAsync(id);
            _db.ScholarshipQuestions.Remove(scholarshipQuestion);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
