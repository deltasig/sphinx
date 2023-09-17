namespace Dsp.WebCore.Areas.Kitchen.Models
{
    using Dsp.Data.Entities;
    using Dsp.WebCore.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MealIndexModel
    {
        public int WeekOffset { get; set; }
        public DateTime StartOfWeek { get; set; }
        public IEnumerable<MealPeriod> MealPeriods { get; set; }
        public IEnumerable<MealItemToPeriod> MealPeriodItems { get; set; }
        public IEnumerable<MealPlate> Plates { get; set; }
        public IEnumerable<MealItem> MealItems { get; set; }
        public IList<MealScheduleRow> Rows { get; set; }
        public IList<DateTime> DistinctDates { get; set; }
        public DateTime StartDate { get; set; }

        public bool HasElevatedPermissions { get; set; }
        public bool ShowEditScheduleLink { get; set; }
        public bool ShowMealItemsLink { get; set; }
        public bool ShowMealPeriodsLink { get; set; }

        public bool ShowSunday { get; set; }
        public bool ShowSaturday { get; set; }

        public MealIndexModel(
            IEnumerable<MealPeriod> mealPeriods,
            IEnumerable<MealItemToPeriod> mealPeriodItems,
            IEnumerable<MealPlate> plates,
            DateTime startDate,
            bool hasElevatedPermissions = false)
        {
            MealPeriods = mealPeriods;
            MealPeriodItems = mealPeriodItems;
            Plates = plates;
            Rows = new List<MealScheduleRow>();
            DistinctDates = new List<DateTime>();
            MealItems = new List<MealItem>();
            StartDate = startDate;

            HasElevatedPermissions = hasElevatedPermissions;
            ShowEditScheduleLink = HasElevatedPermissions;
            ShowMealItemsLink = false;
            ShowMealPeriodsLink = HasElevatedPermissions;

            if (!mealPeriods.Any()) return;

            var orderedMealPeriods = MealPeriods.OrderBy(m => m.StartTime).ToList();
            var orderedMealPeriodItems = mealPeriodItems.OrderBy(m => m.Date).ThenBy(m => m.MealPeriod.StartTime).ToList();
            var firstMpi = orderedMealPeriodItems.FirstOrDefault();
            var lastMpi = orderedMealPeriodItems.LastOrDefault();
            if (firstMpi != null) ShowSunday = firstMpi.Date.DayOfWeek == DayOfWeek.Sunday;
            if (lastMpi != null) ShowSaturday = lastMpi.Date.DayOfWeek == DayOfWeek.Saturday;

            for (var c = 0; c < 7; c++)
            {
                var date = startDate.AddDays(c);
                if (!ShowSunday && date.DayOfWeek == DayOfWeek.Sunday) continue;
                if (!ShowSaturday && date.DayOfWeek == DayOfWeek.Saturday) continue;
                DistinctDates.Add(startDate.AddDays(c));
            }

            // Just enumerate all table placeholders
            for (var p = 0; p < orderedMealPeriods.Count; p++)
            {
                var period = orderedMealPeriods[p];
                var row = new MealScheduleRow(this, period);

                for (var d = 0; d < DistinctDates.Count; d++)
                {
                    var entry = new MealScheduleEntry(row, DistinctDates[d]);
                    row.Columns.Add(entry);
                }
                Rows.Add(row);
            }

            // Fill in meal items iterating across each column, then down through each period, to follow ordering of meal items
            var i = 0;
            for (var d = 0; d < DistinctDates.Count; d++)
            {
                for (var p = 0; p < orderedMealPeriods.Count; p++)
                {
                    var period = orderedMealPeriods[p];
                    while (i < orderedMealPeriodItems.Count)
                    {
                        var pItem = orderedMealPeriodItems[i];

                        if (pItem.MealPeriodId != period.Id || pItem.Date != DistinctDates[d]) break;

                        Rows[p].Columns[d].Items.Add(pItem);
                        i++;
                    }
                }
            }
        }
    }

    public class MealScheduleRow
    {
        private readonly MealIndexModel _parent;

        public MealPeriod Period { get; set; }
        public IList<MealScheduleEntry> Columns { get; set; }

        public MealScheduleRow(
            MealIndexModel parent,
            MealPeriod period)
        {
            _parent = parent;
            Period = period;
            Columns = new List<MealScheduleEntry>();

            Period.StartTime = Period.StartTime.FromUtcToCst();
            Period.EndTime = Period.EndTime.FromUtcToCst();
        }
    }

    public class MealScheduleEntry
    {
        private readonly MealScheduleRow _parent;

        public DateTime Date { get; set; }
        public IList<MealItemToPeriod> Items { get; set; }

        public MealScheduleEntry(MealScheduleRow parent, DateTime date)
        {
            _parent = parent;
            Date = date;
            Items = new List<MealItemToPeriod>();
        }
    }
}