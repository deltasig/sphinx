using Dsp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dsp.Services.Models
{
    public class ServiceGeneralHistoricalStats
    {
        public int SemesterId { get; }
        public string SemesterName { get; }
        public int NumberOfEvents { get; }
        public double TotalHours { get; }
        public double AverageHours { get; }
        public int BiggestEventId { get; }
        public string BiggestEventName { get; }
        public DateTime CalculatedOn { get; }

        public ServiceGeneralHistoricalStats(
            Semester semester,
            IEnumerable<User> nonExemptMembers,
            DateTime calculatedOn)
        {
            SemesterId = semester.Id;
            SemesterName = semester.ToString();

            var approvedServiceEvents = semester.ServiceEvents
                .Where(x => x.IsApproved);
            NumberOfEvents = approvedServiceEvents.Count();
            TotalHours = approvedServiceEvents
                .SelectMany(x => x.ServiceHours)
                .Where(x => nonExemptMembers.Contains(x.User))
                .Sum(x => x.DurationHours);
            AverageHours = TotalHours / nonExemptMembers.Count();
            AverageHours = Math.Round(AverageHours, 2);
            var biggestEvent = approvedServiceEvents
                .OrderByDescending(x => x.ServiceHours.Sum(h => h.DurationHours))
                .FirstOrDefault();
            if (biggestEvent != null)
            {
                BiggestEventId = biggestEvent.EventId;
                BiggestEventName = biggestEvent.EventName;
            }

            CalculatedOn = calculatedOn;
        }
    }
}
