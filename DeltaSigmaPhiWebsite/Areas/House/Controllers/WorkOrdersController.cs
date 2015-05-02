namespace DeltaSigmaPhiWebsite.Areas.House.Controllers
{
    using DeltaSigmaPhiWebsite.Controllers;
    using Entities;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Models;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Administrator")]
    public class WorkOrdersController : BaseController
    {
        public async Task<ActionResult> Index(string s, string sort = "newest", int page = 1, bool open = true, bool closed = false)
        {
            if (page < 1)
            {
                page = 1;
            }

            ViewBag.CurrentFilter = s;
            ViewBag.Open = open;
            ViewBag.Closed = closed;

            var workOrders = await _db.WorkOrders.ToListAsync();
            switch (sort)
            {
                case "newest":
                    workOrders = workOrders.OrderByDescending(o => o.GetDateTimeCreated()).ToList();
                    break;
                case "oldest":
                    workOrders = workOrders.OrderBy(o => o.GetDateTimeCreated()).ToList();
                    break;
                case "most-commented":
                    workOrders = workOrders.OrderBy(o => o.Comments.Max()).ToList();
                    break;
                case "least-commented":
                    workOrders = workOrders.OrderBy(o => o.Comments.Min()).ToList();
                    break;
                case "recently-updated":
                    workOrders = workOrders.OrderByDescending(o => o.GetMostRecentActivityDateTime()).ToList();
                    break;
                case "least-recently-updated":
                    workOrders = workOrders.OrderBy(o => o.GetMostRecentActivityDateTime()).ToList();
                    break;
                default:
                    sort = "newest";
                    workOrders = workOrders.OrderByDescending(o => o.GetDateTimeCreated()).ToList();
                    break;
            }
            ViewBag.Sort = sort;
            ViewBag.OpenResultCount = 0;
            ViewBag.ClosedResultCount = 0;
            
            if (!String.IsNullOrEmpty(s))
            {
                s = s.ToLower();
                workOrders = workOrders
                    .Where(w => 
                        w.WorkOrderId.ToString() == s ||
                        w.Title.ToLower().Contains(s) ||
                        w.Member.FirstName.ToLower().Contains(s) ||
                        w.Member.LastName.ToLower().Contains(s) ||
                        w.GetCurrentPriority().ToLower().Contains(s) ||
                        w.GetCurrentStatus().ToLower().Contains(s))
                    .ToList();
            }

            var filterResults = new List<WorkOrder>();
            if (open)
            {
                var openResults = workOrders.Where(w => w.GetCurrentStatus() != "Closed").ToList();
                ViewBag.OpenResultCount = openResults.Count();
                filterResults.AddRange(openResults);
            }
            if (closed)
            {
                var closedResults = workOrders.Where(w => w.GetCurrentStatus() == "Closed").ToList();
                ViewBag.ClosedResultCount = closedResults.Count();
                filterResults.AddRange(closedResults);
            }

            const int pageSize = 10; // Can make this modifiable in future.
            ViewBag.ResultCount = filterResults.Count;
            ViewBag.PageSize = pageSize;
            ViewBag.Pages = filterResults.Count / pageSize;
            ViewBag.Page = page;
            if (filterResults.Count % pageSize != 0)
            {
                ViewBag.Pages += 1;
            }
            if (page > ViewBag.Pages)
            {
                ViewBag.Page = ViewBag.Pages;
            }
            return View(filterResults.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        public async Task<ActionResult> View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.WorkOrders.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }
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
            model.UserId = WebSecurity.CurrentUserId;
            model.Title = base.ToTitleCaseString(model.Title);
            _db.WorkOrders.Add(model);
            await _db.SaveChangesAsync();

            var initialStatusChange = new WorkOrderStatusChange
            {
                UserId = model.UserId,
                WorkOrderId = model.WorkOrderId,
                ChangedOn = DateTime.UtcNow,
                WorkOrderStatusId = initialStatus.WorkOrderStatusId
            };
            _db.WorkOrderStatusChanges.Add(initialStatusChange);

            var initialPriorityChange = new WorkOrderPriorityChange
            {
                UserId = model.UserId,
                WorkOrderId = model.WorkOrderId,
                ChangedOn = DateTime.UtcNow,
                WorkOrderPriorityId = initialPriority.WorkOrderPriorityId
            };
            _db.WorkOrderPriorityChanges.Add(initialPriorityChange);

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public ActionResult CreateOld()
        {
            var model = new WorkOrderArchiveModel
            {
                WorkOrder = new WorkOrder(),
                SubmittedOn = base.ConvertUtcToCst(DateTime.UtcNow).AddDays(-1),
                ClosedOn = base.ConvertUtcToCst(DateTime.UtcNow)
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
            model.WorkOrder.UserId = WebSecurity.CurrentUserId;
            model.WorkOrder.Title = base.ToTitleCaseString(model.WorkOrder.Title);
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

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.WorkOrders.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(WorkOrder model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            model.Title = base.ToTitleCaseString(model.Title);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = await _db.WorkOrders.FindAsync(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var model = await _db.WorkOrders.FindAsync(id);
            _db.WorkOrders.Remove(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}