namespace DeltaSigmaPhiWebsite.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Data.UnitOfWork;
	using Models;
	using Models.Entities;
	using Models.ViewModels;

	[Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
	public class ServiceHoursController : BaseController
	{
		public ServiceHoursController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

		[HttpGet]
		public ActionResult Index()
		{
			var activeMembers = uow.MemberRepository.SelectAll()
				.Where(m => 
                    m.MemberStatus.StatusName == "Active" || 
                    m.MemberStatus.StatusName == "Pledge" ||
                    m.MemberStatus.StatusName == "Neophyte")
                .ToList();
			
			var model = new List<ServiceIndexModel>();
            var lastSemester = GetLastSemester();
			var thisSemester = GetThisSemester();

			foreach (var member in activeMembers)
			{
				model.Add(new ServiceIndexModel
				{
					Member = member,
					Hours = member.ServiceHours
						.Where(e => 
                            e.Event.DateTimeOccurred > lastSemester.DateEnd && 
							e.DateTimeSubmitted <= thisSemester.DateEnd)
						.Sum(h => h.DurationHours),
					Semester = thisSemester
				});
			}

			return View(model);
		}

		[HttpGet]
		public ActionResult Submit()
		{
			var model = new ServiceHourSubmissionModel { Events = GetAllEventIdsAsEventName() };
			return PartialView(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Submit(ServiceHourSubmissionModel model)
        {
            var message = "";
            // Invalid value
            if (model.HoursServed < 0)
            {
                message = "Please enter a number of hours to submit greater than 0.";
                return RedirectToAction("Index", "Sphinx", new { message });
            }

			var userId = WebSecurity.GetUserId(User.Identity.Name);
            
			// Check if hours submitted is more than held for event
			var selectedEvent = uow.EventRepository.Single(e => e.EventId == model.SelectedEventId);
			if (model.HoursServed > selectedEvent.DurationHours)
			{
                message = "Maximum submission for " + selectedEvent.EventName + " is " + selectedEvent.DurationHours + " hours.";
                return RedirectToAction("Index", "Sphinx", new { message });
			}
            
			// Check if submission has already been created
			var duplicateSubmission = uow.ServiceHourRepository.SelectBy(e => (e.EventId == model.SelectedEventId && e.UserId == userId));
			if (duplicateSubmission.Any())
			{
                // Delete existing record
                if (model.HoursServed == 0)
                {
                    uow.ServiceHourRepository.Delete(duplicateSubmission.Single());
                    uow.Save();
                    message = "Your submission was deleted.";
                    return RedirectToAction("Index", "Sphinx", new { message });
                }

				// Update submission
				duplicateSubmission.First().DurationHours = model.HoursServed;
				uow.Save();
                message = "Your hours for" + selectedEvent.EventName + " event have been updated.";
                return RedirectToAction("Index", "Sphinx", new { message });
			}

            // No existing, invalid value
            if (model.HoursServed == 0)
            {
                message = "Please enter a number of hours to submit greater than 0.";
                return RedirectToAction("Index", "Sphinx", new { message });
            }

			// If no previous submission, create new entry and add it to database
			var submission = new ServiceHour
			{
				UserId = userId,
				DateTimeSubmitted = DateTime.UtcNow,
				EventId = model.SelectedEventId,
				DurationHours = model.HoursServed
			};
			uow.ServiceHourRepository.Insert(submission);
			uow.Save();
            message = "Service hours submitted successfully.";
            return RedirectToAction("Index", "Sphinx", new { message });
		}
	}
}