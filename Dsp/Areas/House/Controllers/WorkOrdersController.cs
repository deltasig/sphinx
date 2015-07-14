﻿namespace Dsp.Areas.House.Controllers
{
    using Entities;
    using global::Dsp.Controllers;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Routing;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class WorkOrdersController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(WorkOrderMessageId? message,
            string s, string sort = "newest", int page = 1, bool open = true, bool closed = false)
        {
            switch (message)
            {
                case WorkOrderMessageId.DeleteSuccess:
                    ViewBag.SuccessMessage = GetWorkOrderResultMessage(message);
                    break;
            }
            var workOrders = await _db.WorkOrders.ToListAsync();
            const int pageSize = 10;
            var filterResults = base.GetFilteredWorkOrderList(workOrders, s, sort, page, open, closed, pageSize);

            // Build view model with collected data.
            var model = new WorkOrderIndexModel
            {
                WorkOrders = filterResults,
                UserWorkOrders = new MyWorkOrdersModel
                {
                    CreatedWorkOrders = workOrders.Where(w =>
                        w.UserId == User.Identity.GetUserId<int>() &&
                        w.GetCurrentStatus() != "Closed"),
                    InvolvedWorkOrders = workOrders.Where(w =>
                        w.Comments.Any(c => c.UserId == User.Identity.GetUserId<int>()) &&
                        w.GetCurrentStatus() != "Closed")
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> View(int? id, WorkOrderMessageId? message)
        {
            switch (message)
            {
                case WorkOrderMessageId.CreateSuccess:
                case WorkOrderMessageId.UpdateSuccess:
                case WorkOrderMessageId.NowUnderReview:
                case WorkOrderMessageId.CommentAdded:
                case WorkOrderMessageId.PriorityChangeSuccess:
                case WorkOrderMessageId.StatusChangeSuccess:
                case WorkOrderMessageId.ClosedSuccess:
                    ViewBag.SuccessMessage = GetWorkOrderResultMessage(message);
                    break;
                case WorkOrderMessageId.CommentTextRequired:
                case WorkOrderMessageId.CommentTextRequiredWhenClosing:
                case WorkOrderMessageId.AlreadyClosed:
                    ViewBag.FailMessage = GetWorkOrderResultMessage(message);
                    break;
            }
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var workOrder = await _db.WorkOrders.FindAsync(id);
            if (workOrder == null) return HttpNotFound();

            if (User.IsInRole("Administrator") && workOrder.GetCurrentStatus() == "Unread")
            {
                var underReviewStatus = await _db.WorkOrderStatuses.SingleAsync(s => s.Name == "Under Review");
                await UpdateWorkOrderStatus(workOrder, underReviewStatus);
                return RedirectToAction("View", new { id, message = WorkOrderMessageId.NowUnderReview });
            }

            var workOrders = await _db.WorkOrders.ToListAsync();
            var model = new WorkOrderViewModel
            {
                WorkOrder = workOrder,
                UserWorkOrders = new MyWorkOrdersModel
                {
                    CreatedWorkOrders = workOrders.Where(w =>
                        w.UserId == User.Identity.GetUserId<int>() &&
                        w.GetCurrentStatus() != "Closed"),
                    InvolvedWorkOrders = workOrders.Where(w =>
                        w.Comments.Any(c => c.UserId == User.Identity.GetUserId<int>()) &&
                        w.GetCurrentStatus() != "Closed")
                }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeWorkOrderStatus(string typeName, int? id)
        {
            if (string.IsNullOrEmpty(typeName) || id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var status = await _db.WorkOrderStatuses
                .SingleOrDefaultAsync(w => w.Name == typeName);
            if (status == null) return HttpNotFound();

            var workOrder = await _db.WorkOrders.FindAsync(id);
            if (workOrder == null) return HttpNotFound();

            var newStatus = new WorkOrderStatusChange
            {
                UserId = User.Identity.GetUserId<int>(),
                ChangedOn = DateTime.UtcNow,
                WorkOrderId = (int) id,
                WorkOrderStatusId = status.WorkOrderStatusId
            };

            _db.WorkOrderStatusChanges.Add(newStatus);
            await _db.SaveChangesAsync();

            return RedirectToAction("View", new { id, message = WorkOrderMessageId.StatusChangeSuccess  });
        }

        [HttpPost]
        public async Task<ActionResult> ChangeWorkOrderPriority(string typeName, int? id)
        {
            if (string.IsNullOrEmpty(typeName) || id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var priority = await _db.WorkOrderPriorities
                .SingleOrDefaultAsync(w => w.Name == typeName);
            if (priority == null) return HttpNotFound();

            var workOrder = await _db.WorkOrders.FindAsync(id);
            if (workOrder == null) return HttpNotFound();

            var newPriority = new WorkOrderPriorityChange
            {
                UserId = User.Identity.GetUserId<int>(),
                ChangedOn = DateTime.UtcNow,
                WorkOrderId = (int)id,
                WorkOrderPriorityId = priority.WorkOrderPriorityId
            };

            _db.WorkOrderPriorityChanges.Add(newPriority);
            await _db.SaveChangesAsync();

            return RedirectToAction("View", new { id, message = WorkOrderMessageId.PriorityChangeSuccess });
        }

        [HttpPost]
        public async Task<ActionResult> Comment(int? workOrderId, string comment, bool close)
        {
            var userId = User.Identity.GetUserId<int>();
            // Perform some crucial server-side validation.
            if (workOrderId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (close && string.IsNullOrEmpty(comment))
            {
                return RedirectToAction("View", new
                {
                    id = workOrderId, 
                    message = WorkOrderMessageId.CommentTextRequiredWhenClosing
                });
            }
            if (string.IsNullOrEmpty(comment))
            {
                return RedirectToAction("View", new
                {
                    id = workOrderId, 
                    message = WorkOrderMessageId.CommentTextRequired
                });
            }
            var workOrder = await _db.WorkOrders.FindAsync(workOrderId);
            if (workOrder == null) return HttpNotFound();
            if (workOrder.GetCurrentStatus() == "Closed")
            {
                return RedirectToAction("View", new
                {
                    id = workOrderId, 
                    message = WorkOrderMessageId.AlreadyClosed
                });
            }

            var commentTime = DateTime.UtcNow;
            var message = WorkOrderMessageId.CommentAdded;
            if (close)
            {
                workOrder.Result = comment;
                var closedStatus = await _db.WorkOrderStatuses.SingleAsync(w => w.Name == "Closed");
                var statusChange = new WorkOrderStatusChange
                {
                    WorkOrderStatusId = closedStatus.WorkOrderStatusId,
                    ChangedOn = commentTime.AddSeconds(1),
                    WorkOrderId = (int) workOrderId,
                    UserId = userId
                };

                _db.Entry(workOrder).State = EntityState.Modified;
                _db.WorkOrderStatusChanges.Add(statusChange);
                await _db.SaveChangesAsync();
                message = WorkOrderMessageId.ClosedSuccess;
            }

            var newComment = new WorkOrderComment
            {
                WorkOrderId = (int) workOrderId,
                SubmittedOn = commentTime,
                UserId = userId,
                Text = comment
            };

            _db.WorkOrderComments.Add(newComment);
            await _db.SaveChangesAsync();

            return RedirectToAction("View", new { id = workOrderId, message = message });
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
            model.UserId = User.Identity.GetUserId<int>();
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

            return RedirectToAction("View", new
            {
                id = model.WorkOrderId, 
                message = WorkOrderMessageId.CreateSuccess
            });
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

            return RedirectToAction("View", new
            {
                id = model.WorkOrder.WorkOrderId,
                message = WorkOrderMessageId.CreateSuccess
            });
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
            return RedirectToAction("View", new
            {
                id = model.WorkOrderId,
                message = WorkOrderMessageId.UpdateSuccess
            });
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
            return RedirectToAction("Index", new { message = WorkOrderMessageId.DeleteSuccess });
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

        public static dynamic GetWorkOrderResultMessage(WorkOrderMessageId? message)
        {
            return message == WorkOrderMessageId.UpdateSuccess ? "Work Order updated successfully."
                : message == WorkOrderMessageId.CreateSuccess ? "Work Order created successfully."
                : message == WorkOrderMessageId.DeleteSuccess ? "Work Order deleted successfully."
                : message == WorkOrderMessageId.NowUnderReview ? "Work Order is now automatically under review!"
                : message == WorkOrderMessageId.CommentTextRequired ? "You must enter some text in the comment field."
                : message == WorkOrderMessageId.CommentTextRequiredWhenClosing ? "You must provide a comment when closing a Work Order."
                : message == WorkOrderMessageId.AlreadyClosed ? "Work Order is already closed."
                : message == WorkOrderMessageId.CommentAdded ? "Comment submitted for Work Order."
                : message == WorkOrderMessageId.PriorityChangeSuccess ? "Work Order priority changed successfully."
                : message == WorkOrderMessageId.StatusChangeSuccess ? "Work Order status changed successfully."
                : message == WorkOrderMessageId.ClosedSuccess ? "Work Order has been closed!"
                : "";
        }

        public enum WorkOrderMessageId
        {
            UpdateSuccess,
            CreateSuccess,
            DeleteSuccess,
            NowUnderReview,
            CommentTextRequired,
            CommentTextRequiredWhenClosing,
            AlreadyClosed,
            CommentAdded,
            PriorityChangeSuccess,
            StatusChangeSuccess,
            ClosedSuccess
        }
    }
}