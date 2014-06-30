﻿namespace DeltaSigmaPhiWebsite.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using Models.ViewModels;

    [Authorize(Roles = "Pledge, Neophyte, Active")]
    public class ServiceController : BaseController
    {
        public ServiceController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SubmitService()
        {
            var model = new ServiceHourSubmissionModel { Events = GetAllEventIdsAsEventName() };
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitService(ServiceHourSubmissionModel model)
        {
            var userId = WebSecurity.GetUserId(User.Identity.Name);

            // Check if hours submitted is more than held for event
            var selectedEvent = uow.EventRepository.Get(e => e.EventId == model.SelectedEventId);
            if (model.HoursServed > selectedEvent.DurationHours)
            {
                TempData["ServiceSubmissionError"] = "Maximum submission for " + selectedEvent.EventName + " is " + selectedEvent.DurationHours + " hours.";
                return RedirectToAction("Index");
            }

            if (model.HoursServed <= 0)
            {
                TempData["ServiceSubmissionError"] = "Please enter a number of hours to submit greater than 0.";
                return RedirectToAction("Index");
            }

            // Check if submission has already been created under same eventId for userId
            var duplicateSubmission = uow.ServiceHourRepository.FindBy(e => (e.EventId == model.SelectedEventId && e.UserId == userId));
            if (duplicateSubmission.Any())
            {
                // Previous submission found
                duplicateSubmission.First().DurationHours = model.HoursServed;
                uow.Save();
                return RedirectToAction("Index");
            }

            // If no previous submission, create new entry and add it to database
            var submission = new ServiceHour
            {
                UserId = userId,
                DateTimeSubmitted = DateTime.Now,
                EventId = model.SelectedEventId,
                DurationHours = model.HoursServed
            };
            uow.ServiceHourRepository.Insert(submission);
            uow.Save();
            return RedirectToAction("Index");
        }

    }
}