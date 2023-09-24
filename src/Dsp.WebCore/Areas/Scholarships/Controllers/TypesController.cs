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
public class TypesController : BaseController
{
    public async Task<ActionResult> Index()
    {
        ViewBag.SuccessMessage = TempData[SuccessMessageKey];
        ViewBag.FailureMessage = TempData[FailureMessageKey];
        return View(await Context.ScholarshipTypes.ToListAsync());
    }
    
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(ScholarshipType model)
    {
        if (!ModelState.IsValid) return View(model);

        Context.ScholarshipTypes.Add(model);
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Scholarship Type created successfully.";
        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        var scholarshiptype = await Context.ScholarshipTypes.FindAsync(id);
        if (scholarshiptype == null)
        {
            return NotFound();
        }
        return View(scholarshiptype);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(ScholarshipType model)
    {
        if (!ModelState.IsValid) return View(model);

        Context.Entry(model).State = EntityState.Modified;
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Scholarship Type updated successfully.";
        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        }
        var scholarshiptype = await Context.ScholarshipTypes.FindAsync(id);
        if (scholarshiptype == null || scholarshiptype.Applications.Any())
        {
            return NotFound();
        }
        return View(scholarshiptype);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        var scholarshipType = await Context.ScholarshipTypes.FindAsync(id);
        if (scholarshipType.Applications.Any())
        {
            TempData[FailureMessageKey] = "The " + scholarshipType.Name +
                " Scholarship Type could not be deleted because it has existing applications associated with it.";
            return RedirectToAction("Index");
        }

        Context.ScholarshipTypes.Remove(scholarshipType);
        await Context.SaveChangesAsync();

        TempData[SuccessMessageKey] = "Scholarship Type deleted successfully.";
        return RedirectToAction("Index");
    }

}
