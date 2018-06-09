namespace Dsp.Web.Areas.Laundry.Models
{
    using Dsp.Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class LaundrySchedule
    {
        private const int DayLength = 24;
        private readonly IEnumerable<LaundrySignup> _existingSignups;

        public int WindowSize { get; set; }
        public int SlotSize { get; set; }
        public DateTime Now { get; set; }
        public DateTime StartOfToday { get; set; }
        public List<LaundryRow> Rows { get; set; }

        public LaundrySchedule(DateTime now, int windowSize, int slotSize, IEnumerable<LaundrySignup> existingSignups)
        {
            Now = now;
            StartOfToday = now.Date;
            // Ensure that windowSize is from 1-7 days.
            WindowSize = windowSize >= 1 && windowSize <= 7 ? windowSize : 7;
            // Ensure that slotSize is from 2-24 and a multiple of 2.
            SlotSize = slotSize % 2 == 0 && slotSize <= 24 && slotSize >= 2 ? slotSize : 2;
            _existingSignups = existingSignups;
            Rows = new List<LaundryRow>();
            PopulateSchedule();
        }

        private void PopulateSchedule()
        {
            var rowCount = DayLength / SlotSize;
            for (var h = 0; h < rowCount; h++) // Hours in day = DayLength/SlotSize = # of rows
            {
                var row = new LaundryRow();
                var firstTimeSlot = StartOfToday.AddHours(h * SlotSize);
                row.Label = firstTimeSlot.ToString("hh:mm tt") + " to " +
                            firstTimeSlot.AddHours(SlotSize).AddMinutes(-1).ToString("hh:mm tt");
                for (var d = 0; d < WindowSize; d++) // Days of week = WindowSize = # of signups in row
                {
                    var slot = StartOfToday.AddHours(h * SlotSize + d * DayLength);
                    var signup = new LaundrySignup { DateTimeShift = slot };
                    var existingSignup = _existingSignups.SingleOrDefault(s => s.DateTimeShift == slot);
                    if (existingSignup != null)
                    {
                        signup.DateTimeSignedUp = existingSignup.DateTimeSignedUp;
                        signup.UserId = existingSignup.UserId;
                        signup.Member = existingSignup.Member;
                    }
                    row.Signups.Add(signup);
                }
                Rows.Add(row);
            }
        }

        public class LaundryRow
        {
            public string Label { get; set; }
            public List<LaundrySignup> Signups { get; set; }

            public LaundryRow()
            {
                Signups = new List<LaundrySignup>();
            }
        }
    }
}