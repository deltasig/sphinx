namespace Dsp.WebCore.Areas.House.Controllers;

using Dsp.Data.Entities;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Controllers;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

[Authorize(Roles = "New, Neophyte, Active, Alumnus, Administrator, ACB House Manager")]
public class WorkOrdersController : BaseController
{
    private readonly IPositionService _positionService;

    public WorkOrdersController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    [HttpGet]
    public async Task<ActionResult> Index(WorkOrderIndexFilterModel filter)
    {
        var workOrders = await Context.WorkOrders.ToListAsync();
        var filterResults = GetFilteredWorkOrderList(
            workOrders,
            filter.s,
            filter.sort,
            filter.page,
            filter.open,
            filter.closed
        );
        var currentUserId = User.GetUserId();
        var usersWorkOrders = workOrders
            .Where(x => x.ClosedOn == null && x.UserId == currentUserId);
        var totalPages = ViewBag.Pages;
        var openCount = ViewBag.OpenResultCount;
        var closedCount = ViewBag.ClosedResultCount;

        var model = new WorkOrderIndexModel(
            filterResults,
            usersWorkOrders,
            filter,
            totalPages,
            openCount,
            closedCount
        );

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailureMessage = TempData["FailureMessage"];

        return View(model);
    }

    [HttpGet]
    public async Task<ActionResult> Details(int? id)
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailureMessage = TempData["FailureMessage"];

        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var workOrder = await Context.WorkOrders.FindAsync(id);
        if (workOrder == null) return NotFound();

        return View(workOrder);
    }

    [HttpGet]
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(WorkOrder model)
    {
        if (!ModelState.IsValid) return View(model);

        var member = await UserManager.FindByIdAsync(User.GetUserId().ToString());
        model.UserId = member.Id;
        model.Title = ToTitleCaseString(model.Title);
        Context.WorkOrders.Add(model);
        await Context.SaveChangesAsync();

        // Send email to house manager.
        var houseMan = await GetCurrentLeader("House Manager");
        if (houseMan == null)
        {

            TempData["SuccessMessage"] = "Work order created successfully but no email was " +
                "sent to the house manager because no one is currently appointed.";
            return RedirectToAction("Details", new { id = model.WorkOrderId });
        }
        model.User = member;
        var body = RenderRazorViewToString("~/Views/Emails/NewWorkOrder.cshtml", model);
        var bytes = Encoding.Default.GetBytes(body);
        body = Encoding.UTF8.GetString(bytes);

        // TODO: Send Email(s)

        TempData["SuccessMessage"] = "Work order created successfully.";
        return RedirectToAction("Details", new { id = model.WorkOrderId });
    }

    [HttpGet]
    public ActionResult CreateOld()
    {
        var model = new WorkOrderArchiveModel
        {
            WorkOrder = new WorkOrder(),
            SubmittedOn = DateTime.UtcNow.FromUtcToCst().AddDays(-1),
            ClosedOn = DateTime.UtcNow.FromUtcToCst()
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> CreateOld(WorkOrderArchiveModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Insert work item.  Doing this after we make sure we have the status and priority Ids.
        model.WorkOrder.UserId = User.GetUserId();
        model.WorkOrder.Title = ToTitleCaseString(model.WorkOrder.Title);
        Context.WorkOrders.Add(model.WorkOrder);
        await Context.SaveChangesAsync();

        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Past work order created and archived successfuly.";
        return RedirectToAction("Details", new { id = model.WorkOrder.WorkOrderId });
    }

    [HttpGet]
    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var model = await Context.WorkOrders.FindAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(WorkOrder model)
    {
        if (!ModelState.IsValid) return View(model);

        Context.Entry(model).State = EntityState.Modified;
        model.Title = ToTitleCaseString(model.Title);
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Work order updated successfully.";
        return RedirectToAction("Details", new { id = model.WorkOrderId });
    }

    [HttpGet]
    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null) return new StatusCodeResult((int) HttpStatusCode.BadRequest);
        var model = await Context.WorkOrders.FindAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        var model = await Context.WorkOrders.FindAsync(id);
        Context.WorkOrders.Remove(model);
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Work order deleted successfully.";
        return RedirectToAction("Index");
    }
}