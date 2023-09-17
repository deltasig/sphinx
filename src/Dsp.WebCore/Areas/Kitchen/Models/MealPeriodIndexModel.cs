using Dsp.Data.Entities;
using Dsp.WebCore.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Dsp.WebCore.Areas.Kitchen.Models
{
    public class MealPeriodIndexModel
    {
        public IEnumerable<MealPeriod> MealPeriods { get; set; }
        public IList<MealPeriodContent> TableContents { get; set; }

        public MealPeriodIndexModel(IEnumerable<MealPeriod> mealPeriods)
        {
            MealPeriods = mealPeriods;
            TableContents = new List<MealPeriodContent>();
            foreach (var e in MealPeriods)
            {
                TableContents.Add(new MealPeriodContent(e));
            }
        }

        public class MealPeriodContent
        {
            public MealPeriod Entity { get; set; }
            public bool AllowEdit { get; set; }
            public bool AllowDelete { get; set; }

            public MealPeriodContent(MealPeriod entity)
            {
                Entity = entity;
                entity.StartTime = entity.StartTime.FromUtcToCst();
                entity.EndTime = entity.StartTime.FromUtcToCst();
                AllowEdit = true;
                AllowDelete = !entity.Items.Any();
            }
        }
    }
}