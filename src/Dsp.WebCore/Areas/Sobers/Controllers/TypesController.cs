namespace Dsp.WebCore.Areas.Sobers.Controllers;

using Dsp.Data.Entities;
using Dsp.WebCore.Controllers;
using MarkdownSharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.Tasks;

[Area("Sobers")]
[Authorize]
public class TypesController : BaseController
{
    [Authorize]
    public async Task<ActionResult> Index()
    {
        var types = await Context.SoberTypes.Include(m => m.Signups).ToListAsync();

        var markdown = new Markdown();
        foreach (var type in types)
        {
            type.Description = markdown.Transform(type.Description);
        }

        return View(types);
    }

    [Authorize]
    public ActionResult Create()
    {
        return View();
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(SoberType soberType)
    {
        if (!ModelState.IsValid) return View(soberType);

        Context.SoberTypes.Add(soberType);
        await Context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [Authorize]
    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        }
        var soberType = await Context.SoberTypes.FindAsync(id);
        if (soberType == null)
        {
            return NotFound();
        }
        return View(soberType);
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(SoberType soberType)
    {
        if (!ModelState.IsValid) return View(soberType);

        Context.Entry(soberType).State = EntityState.Modified;
        await Context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [Authorize]
    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        }
        var soberType = await Context.SoberTypes.FindAsync(id);
        if (soberType == null)
        {
            return NotFound();
        }

        var markdown = new Markdown();
        soberType.Description = markdown.Transform(soberType.Description);

        return View(soberType);
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        var soberType = await Context.SoberTypes.FindAsync(id);
        if (soberType == null)
        {
            return NotFound();
        }
        Context.SoberTypes.Remove(soberType);
        await Context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Details(int? id)
    {
        if (id == null)
        {
            return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        }
        var soberType = await Context.SoberTypes.FindAsync(id);
        if (soberType == null)
        {
            return NotFound();
        }

        var markdown = new Markdown();
        soberType.Description = markdown.Transform(soberType.Description);

        return View(soberType);
    }
}
