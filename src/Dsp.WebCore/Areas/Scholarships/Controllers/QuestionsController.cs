namespace Dsp.WebCore.Areas.Scholarships.Controllers;

using Dsp.Data.Entities;
using Dsp.WebCore.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

[Authorize(Roles = "Administrator, Vice President Growth, Director of Recruitment")]
public class QuestionsController : BaseController
{
    public async Task<ActionResult> Index()
    {
        ViewBag.SuccessMessage = TempData[SuccessMessageKey];
        ViewBag.FailureMessage = TempData[FailureMessageKey];
        return View(await Context.ScholarshipQuestions.ToListAsync());
    }

    public async Task<ActionResult> Details(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var scholarshipQuestion = await Context.ScholarshipQuestions.FindAsync(id);
        if (scholarshipQuestion == null) return NotFound();

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

        Context.ScholarshipQuestions.Add(model);
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Question successfully added to pool.";
        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var model = await Context.ScholarshipQuestions.FindAsync(id);
        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(ScholarshipQuestion model)
    {
        if (!ModelState.IsValid) return View(model);

        Context.Entry(model).State = EntityState.Modified;
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Question successfully modified.";
        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var scholarshipQuestion = await Context.ScholarshipQuestions.FindAsync(id);
        if (scholarshipQuestion == null) return NotFound(); 

        return View(scholarshipQuestion);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        var scholarshipQuestion = await Context.ScholarshipQuestions.FindAsync(id);
        if (scholarshipQuestion.Answers.Any())
        {
            TempData[FailureMessageKey] = 
                "Scholarship Question could not be deleted because it has existing answers associated with it.";
            return RedirectToAction("Index");
        }
        Context.ScholarshipQuestions.Remove(scholarshipQuestion);
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Question successfully deleted.";
        return RedirectToAction("Index");
    }
}
