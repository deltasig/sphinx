using Dsp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dsp.Services.Models
{
    public class ServiceMemberProgress
    {
        public int MemberId { get; }
        public string Name { get { return $"{LastName}, {FirstName}"; } }
        public string FirstName { get; }
        public string LastName { get; }
        public int NewMemberClassSemesterId { get; }
        public double Hours { get; set; }
        public double HoursGoal { get; }
        public int EventsGoal { get; }
        public double Percentage { get; }
        public IEnumerable<ServiceEventProgress> Submissions { get; set; }
        public int ServiceHoursCount { get; }
        public string HourAmendmentsDisplay { get; }
        public int HourAmendmentsCount { get; }
        public double HourAmendmentsTotal { get; }
        public string EventAmendmentsDisplay { get; }
        public int EventAmendmentsCount { get; }
        public int EventAmendmentsTotal { get; }
        public DateTime CalculatedOn { get; } = DateTime.UtcNow;

        public ServiceMemberProgress(
            Semester selectedSemester,
            Member member,
            DateTime calculatedOn)
        {
            MemberId = member.Id;
            FirstName = member.FirstName;
            LastName = member.LastName;
            NewMemberClassSemesterId = member.PledgeClass.SemesterId;

            var serviceHours = member.ServiceHours
                .Where(e =>
                    e.Event.IsApproved &&
                    e.Event.SemesterId == selectedSemester.SemesterId).ToList();
            ServiceHoursCount = serviceHours.Count();
            Hours = serviceHours.Sum(h => h.DurationHours);
            Submissions = serviceHours.Select(x => new ServiceEventProgress(x));

            var hourAmendments = selectedSemester.ServiceHourAmendments.Where(a => a.UserId == member.Id).ToList();
            HourAmendmentsCount = hourAmendments.Count();
            HourAmendmentsTotal = hourAmendments.Sum(a => a.AmountHours);
            var eventAmendments = selectedSemester.ServiceEventAmendments.Where(a => a.UserId == member.Id).ToList();
            EventAmendmentsCount = eventAmendments.Count();
            EventAmendmentsTotal = eventAmendments.Sum(a => a.NumberEvents);
            HoursGoal = selectedSemester.MinimumServiceHours + HourAmendmentsTotal;
            EventsGoal = selectedSemester.MinimumServiceEvents + EventAmendmentsTotal;
            Percentage = Hours * 100.0;
            if (HoursGoal > 0)
            {
                Percentage /= HoursGoal;
            }
            Percentage = Math.Round(Percentage, 2);

            HourAmendmentsDisplay = string.Empty;
            for (var i = 0; i < HourAmendmentsCount; i++)
            {
                var amd = hourAmendments.ElementAt(i);
                HourAmendmentsDisplay = $"{amd.Reason} ({amd.AmountHours} hr{(!amd.AmountHours.Equals(1) ? "s" : "")})";
                if (i < HourAmendmentsCount - 1)
                {
                    HourAmendmentsDisplay += ",";
                }
            }

            EventAmendmentsDisplay = string.Empty;
            for (var i = 0; i < EventAmendmentsCount; i++)
            {
                var amd = eventAmendments.ElementAt(i);
                EventAmendmentsDisplay = $"{amd.Reason} ({amd.NumberEvents} event{(!amd.NumberEvents.Equals(1) ? "s" : "")})";
                if (i < EventAmendmentsCount - 1)
                {
                    EventAmendmentsDisplay += ",";
                }
            }

            CalculatedOn = calculatedOn;
        }
    }

    public class ServiceEventProgress
    {
        public int EventId { get; }
        public string EventName { get; }
        public double EventDuration { get; }
        public double HoursServed { get; }
        public int PercentageOfEvent { get; }

        public ServiceEventProgress(ServiceHour serviceHour)
        {
            EventId = serviceHour.EventId;
            EventName = serviceHour.Event.EventName;
            EventDuration = serviceHour.Event.DurationHours;
            HoursServed = serviceHour.DurationHours;
            PercentageOfEvent = (int)(serviceHour.DurationHours / serviceHour.Event.DurationHours * 100.0);
        }
    }
}
