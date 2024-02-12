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

[Area("House")]
[Authorize]
public class WorkOrdersController : BaseController
{
    private readonly IPositionService _positionService;
    private readonly ISemesterService _semesterService;
    private readonly IMemberService _memberService;

    public WorkOrdersController(IPositionService positionService, ISemesterService semesterService, IMemberService memberService)
    {
        _positionService = positionService;
        _semesterService = semesterService;
        _memberService = memberService;
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

        if (id == null) return new StatusCodeResult((int)HttpStatusCode.BadRequest);
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

        var member = await _memberService.GetMemberByUserNameAsync(User.GetUserId().ToString());
        model.UserId = member.Id;
        model.Title = model.Title.ToTitleCaseString();
        Context.WorkOrders.Add(model);
        await Context.SaveChangesAsync();

        // Send email to house manager.
        var currentSemester = await _semesterService.GetCurrentSemesterAsync();
        var houseMan = await _positionService.GetUserInPositionAsync("House Manager", currentSemester.Id);
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
        model.WorkOrder.Title = model.WorkOrder.Title.ToTitleCaseString();
        Context.WorkOrders.Add(model.WorkOrder);
        await Context.SaveChangesAsync();

        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Past work order created and archived successfuly.";
        return RedirectToAction("Details", new { id = model.WorkOrder.WorkOrderId });
    }

    [HttpGet]
    public async Task<ActionResult> Edit(int? id)
    {
        if (id == null) return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        var model = await Context.WorkOrders.FindAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(WorkOrder model)
    {
        if (!ModelState.IsValid) return View(model);

        Context.Entry(model).State = EntityState.Modified;
        model.Title = model.Title.ToTitleCaseString();
        await Context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Work order updated successfully.";
        return RedirectToAction("Details", new { id = model.WorkOrderId });
    }

    [HttpGet]
    public async Task<ActionResult> Delete(int? id)
    {
        if (id == null) return new StatusCodeResult((int)HttpStatusCode.BadRequest);
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

    protected virtual IEnumerable<WorkOrder> GetFilteredWorkOrderList(
        IList<WorkOrder> workOrders,
        string s, string sort, int page, bool open, bool closed, int pageSize = 10)
    {
        var filterResults = new List<WorkOrder>();
        // Filter out results based on open, closed, sort, and/or search string.
        if (open)
        {
            var openResults = workOrders.Where(w => w.IsOpen).ToList();
            filterResults.AddRange(openResults);
        }
        if (closed)
        {
            var closedResults = workOrders.Where(w => w.IsClosed).ToList();
            filterResults.AddRange(closedResults);
        }
        switch (sort)
        {
            case "oldest":
                filterResults = filterResults.OrderBy(o => o.CreatedOn).ToList();
                break;
            default:
                sort = "newest";
                filterResults = filterResults.OrderByDescending(o => o.CreatedOn).ToList();
                break;
        }
        if (!string.IsNullOrEmpty(s))
        {
            s = s.ToLower();
            filterResults = filterResults
                .Where(w =>
                    w.WorkOrderId.ToString() == s ||
                    w.Title.ToLower().Contains(s) ||
                    w.User.FirstName.ToLower().Contains(s) ||
                    w.User.LastName.ToLower().Contains(s))
                .ToList();
        }
        ViewBag.OpenResultCount = filterResults.Count(w => w.IsOpen);
        ViewBag.ClosedResultCount = filterResults.Count(w => w.IsClosed);

        // Set search values so they carry over to the view.
        ViewBag.CurrentFilter = s;
        ViewBag.Open = open;
        ViewBag.Closed = closed;
        ViewBag.Sort = sort;

        if (page < 1) page = 1;
        ViewBag.Count = filterResults.Count;
        ViewBag.PageSize = pageSize;
        ViewBag.Pages = filterResults.Count / pageSize;
        ViewBag.Page = page;
        if (filterResults.Count % pageSize != 0) ViewBag.Pages += 1;
        if (page > ViewBag.Pages) ViewBag.Page = ViewBag.Pages;

        return filterResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }
}