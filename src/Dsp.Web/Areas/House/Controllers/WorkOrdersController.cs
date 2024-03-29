﻿namespace Dsp.Web.Areas.House.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.Web.Controllers;
    using Extensions;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize(Roles = "New, Neophyte, Active, Alumnus, Administrator, ACB House Manager")]
    public class WorkOrdersController : BaseController
    {
        private readonly IPositionService _positionService;

        public WorkOrdersController()
        {
            var repo = new Repository<SphinxDbContext>(_db);
            _positionService = new PositionService(repo);
        }

        public WorkOrdersController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet]
        public async Task<ActionResult> Index(WorkOrderIndexFilterModel filter)
        {
            var workOrders = await _db.WorkOrders.ToListAsync();
            var filterResults = GetFilteredWorkOrderList(
                workOrders,
                filter.s,
                filter.sort,
                filter.page,
                filter.open,
                filter.closed
            );
            var currentUserId = User.Identity.GetUserId<int>();
            var usersWorkOrders = workOrders
                .Where(x => x.GetCurrentStatus() != "Closed" && x.UserId == currentUserId);
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
        public async Task<ActionResult> View(int? id)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var workOrder = await _db.WorkOrders.FindAsync(id);
            if (workOrder == null) return HttpNotFound();

            var userId = User.Identity.GetUserId<int>();
            var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "House Manager");
            if (hasElevatedPermissions && workOrder.GetCurrentStatus() == "Unread")
            {
                var underReviewStatus = await _db.WorkOrderStatuses.SingleAsync(s => s.Name == "Under Review");
                await UpdateWorkOrderStatus(workOrder, underReviewStatus);
                TempData["SuccessMessage"] = "Work order has been placed under review.";
                return RedirectToAction("View", new { id });
            }

            var workOrders = await _db.WorkOrders.ToListAsync();
            var model = new WorkOrderViewModel
            {
                WorkOrder = workOrder,
                UsersWorkOrders = workOrders.Where(x => x.GetCurrentStatus() != "Closed" && x.UserId == userId)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeWorkOrderStatus(string typeName, int? id)
        {
            if (string.IsNullOrEmpty(typeName) || id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var status = await _db.WorkOrderStatuses.SingleOrDefaultAsync(w => w.Name == typeName);
            if (status == null)
                return HttpNotFound();

            var workOrder = await _db.WorkOrders.FindAsync(id);
            if (workOrder == null)
                return HttpNotFound();

            var newStatus = new WorkOrderStatusChange
            {
                UserId = User.Identity.GetUserId<int>(),
                ChangedOn = DateTime.UtcNow,
                WorkOrderId = (int)id,
                WorkOrderStatusId = status.WorkOrderStatusId
            };

            _db.WorkOrderStatusChanges.Add(newStatus);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Work order status successfully changed to " + status.Name + ".";
            return RedirectToAction("View", new { id });
        }

        [HttpPost]
        public async Task<ActionResult> ChangeWorkOrderPriority(string typeName, int? id)
        {
            if (string.IsNullOrEmpty(typeName) || id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var priority = await _db.WorkOrderPriorities.SingleOrDefaultAsync(w => w.Name == typeName);
            if (priority == null)
                return HttpNotFound();

            var workOrder = await _db.WorkOrders.FindAsync(id);
            if (workOrder == null)
                return HttpNotFound();

            var newPriority = new WorkOrderPriorityChange
            {
                UserId = User.Identity.GetUserId<int>(),
                ChangedOn = DateTime.UtcNow,
                WorkOrderId = (int)id,
                WorkOrderPriorityId = priority.WorkOrderPriorityId
            };

            _db.WorkOrderPriorityChanges.Add(newPriority);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Work order priority successfully changed to " + priority.Name + ".";
            return RedirectToAction("View", new { id });
        }

        [HttpPost]
        public async Task<ActionResult> Comment(int? workOrderId, string comment, bool close)
        {
            var userId = User.Identity.GetUserId<int>();
            // Perform some crucial server-side validation.
            if (workOrderId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (close && string.IsNullOrEmpty(comment))
            {
                TempData["FailureMessage"] = "A comment is required when closing a work order.";
                return RedirectToAction("View", new { id = workOrderId });
            }
            if (string.IsNullOrEmpty(comment))
            {
                TempData["FailureMessage"] = "Empty comments are not allowed.";
                return RedirectToAction("View", new { id = workOrderId });
            }
            var workOrder = await _db.WorkOrders.FindAsync(workOrderId);
            if (workOrder == null) return HttpNotFound();
            if (workOrder.GetCurrentStatus() == "Closed")
            {
                TempData["FailureMessage"] = "This work order is already closed.";
                return RedirectToAction("View", new { id = workOrderId });
            }

            var commentTime = DateTime.UtcNow;
            if (close)
            {
                workOrder.Result = comment;
                var closedStatus = await _db.WorkOrderStatuses.SingleAsync(w => w.Name == "Closed");
                var statusChange = new WorkOrderStatusChange
                {
                    WorkOrderStatusId = closedStatus.WorkOrderStatusId,
                    ChangedOn = commentTime.AddSeconds(1),
                    WorkOrderId = (int)workOrderId,
                    UserId = userId
                };

                _db.Entry(workOrder).State = EntityState.Modified;
                _db.WorkOrderStatusChanges.Add(statusChange);
                await _db.SaveChangesAsync();
            }

            var newComment = new WorkOrderComment
            {
                WorkOrderId = (int)workOrderId,
                SubmittedOn = commentTime,
                UserId = userId,
                Text = comment
            };

            _db.WorkOrderComments.Add(newComment);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = close
                ? "Work order closed successfully."
                : "Comment posted successfully.";
            return RedirectToAction("View", new { id = workOrderId });
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

            // Get initial status and priority values; add them if not in Db.
            var initialStatus = await _db.WorkOrderStatuses.SingleOrDefaultAsync(w => w.Name == "Unread");
            if (initialStatus == null)
            {
                initialStatus = new WorkOrderStatus { Name = "Unread" };
                _db.WorkOrderStatuses.Add(initialStatus);
                await _db.SaveChangesAsync();
            }
            var initialPriority = await _db.WorkOrderPriorities.SingleOrDefaultAsync(w => w.Name == "Low");
            if (initialPriority == null)
            {
                initialPriority = new WorkOrderPriority { Name = "Low" };
                _db.WorkOrderPriorities.Add(initialPriority);
                await _db.SaveChangesAsync();
            }

            // Insert work item.  Doing this after we make sure we have the status and priority Ids.
            var member = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            model.UserId = member.Id;
            model.Title = ToTitleCaseString(model.Title);
            _db.WorkOrders.Add(model);
            await _db.SaveChangesAsync();

            var initialStatusChange = new WorkOrderStatusChange
            {
                UserId = model.UserId,
                WorkOrderId = model.WorkOrderId,
                ChangedOn = DateTime.UtcNow,
                WorkOrderStatusId = initialStatus.WorkOrderStatusId
            };
            var initialPriorityChange = new WorkOrderPriorityChange
            {
                UserId = model.UserId,
                WorkOrderId = model.WorkOrderId,
                ChangedOn = DateTime.UtcNow,
                WorkOrderPriorityId = initialPriority.WorkOrderPriorityId
            };
            _db.WorkOrderStatusChanges.Add(initialStatusChange);
            _db.WorkOrderPriorityChanges.Add(initialPriorityChange);
            await _db.SaveChangesAsync();

            // Send email to house manager.
            var houseMan = await GetCurrentLeader("House Manager");
            if (houseMan == null)
            {

                TempData["SuccessMessage"] = "Work order created successfully but no email was " +
                    "sent to the house manager because no one is currently appointed.";
                return RedirectToAction("View", new { id = model.WorkOrderId });
            }
            model.Member = member;
            var body = RenderRazorViewToString("~/Views/Emails/NewWorkOrder.cshtml", model);
            var bytes = Encoding.Default.GetBytes(body);
            body = Encoding.UTF8.GetString(bytes);

            var message = new IdentityMessage
            {
                Subject = "New Work Order - " + model.Title,
                Body = body,
                Destination = houseMan.Member.Email
            };

            try
            {
                var emailService = new EmailService();
                await emailService.SendAsync(message);
            }
            catch (SmtpException e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            TempData["SuccessMessage"] = "Work order created successfully.";
            return RedirectToAction("View", new { id = model.WorkOrderId });
        }

        [HttpGet]
        public ActionResult CreateOld()
        {
            var model = new WorkOrderArchiveModel
            {
                WorkOrder = new WorkOrder(),
                SubmittedOn = ConvertUtcToCst(DateTime.UtcNow).AddDays(-1),
                ClosedOn = ConvertUtcToCst(DateTime.UtcNow)
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOld(WorkOrderArchiveModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Get initial status and priority values; add them if not in Db.
            var initialStatus = await _db.WorkOrderStatuses.SingleOrDefaultAsync(w => w.Name == "Unread");
            if (initialStatus == null)
            {
                initialStatus = new WorkOrderStatus { Name = "Unread" };
                _db.WorkOrderStatuses.Add(initialStatus);
                await _db.SaveChangesAsync();
            }
            var finalStatus = await _db.WorkOrderStatuses.SingleOrDefaultAsync(w => w.Name == "Closed");
            if (finalStatus == null)
            {
                finalStatus = new WorkOrderStatus { Name = "Closed" };
                _db.WorkOrderStatuses.Add(finalStatus);
                await _db.SaveChangesAsync();
            }
            var initialPriority = await _db.WorkOrderPriorities.SingleOrDefaultAsync(w => w.Name == "Low");
            if (initialPriority == null)
            {
                initialPriority = new WorkOrderPriority { Name = "Low" };
                _db.WorkOrderPriorities.Add(initialPriority);
                await _db.SaveChangesAsync();
            }

            // Insert work item.  Doing this after we make sure we have the status and priority Ids.
            model.WorkOrder.UserId = User.Identity.GetUserId<int>();
            model.WorkOrder.Title = ToTitleCaseString(model.WorkOrder.Title);
            _db.WorkOrders.Add(model.WorkOrder);
            await _db.SaveChangesAsync();

            var initialStatusChange = new WorkOrderStatusChange
            {
                UserId = model.WorkOrder.UserId,
                WorkOrderId = model.WorkOrder.WorkOrderId,
                ChangedOn = model.SubmittedOn,
                WorkOrderStatusId = initialStatus.WorkOrderStatusId
            };
            var finalStatusChange = new WorkOrderStatusChange
            {
                UserId = model.WorkOrder.UserId,
                WorkOrderId = model.WorkOrder.WorkOrderId,
                ChangedOn = model.ClosedOn,
                WorkOrderStatusId = finalStatus.WorkOrderStatusId
            };
            _db.WorkOrderStatusChanges.Add(initialStatusChange);
            _db.WorkOrderStatusChanges.Add(finalStatusChange);

            var initialPriorityChange = new WorkOrderPriorityChange
            {
                UserId = model.WorkOrder.UserId,
                WorkOrderId = model.WorkOrder.WorkOrderId,
                ChangedOn = model.SubmittedOn,
                WorkOrderPriorityId = initialPriority.WorkOrderPriorityId
            };
            _db.WorkOrderPriorityChanges.Add(initialPriorityChange);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Past work order created and archived successfuly.";
            return RedirectToAction("View", new { id = model.WorkOrder.WorkOrderId });
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.WorkOrders.FindAsync(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(WorkOrder model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            model.Title = ToTitleCaseString(model.Title);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Work order updated successfully.";
            return RedirectToAction("View", new { id = model.WorkOrderId });
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = await _db.WorkOrders.FindAsync(id);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await _db.WorkOrders.FindAsync(id);
            _db.WorkOrders.Remove(model);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Work order deleted successfully.";
            return RedirectToAction("Index");
        }

        private async Task UpdateWorkOrderStatus(WorkOrder workOrder, WorkOrderStatus newStatus)
        {
            var statusChange = new WorkOrderStatusChange
            {
                ChangedOn = DateTime.UtcNow,
                UserId = User.Identity.GetUserId<int>(),
                WorkOrderId = workOrder.WorkOrderId,
                WorkOrderStatusId = newStatus.WorkOrderStatusId
            };

            _db.Entry(workOrder).State = EntityState.Modified;
            _db.WorkOrderStatusChanges.Add(statusChange);
            await _db.SaveChangesAsync();
        }
    }
}