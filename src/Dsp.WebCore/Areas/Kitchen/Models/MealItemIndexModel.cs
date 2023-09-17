using Dsp.Data.Entities;
using Dsp.WebCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace Dsp.WebCore.Areas.Kitchen.Models
{
    public class MealItemIndexModel : BaseViewModel
    {
        public IEnumerable<MealItem> MealItems { get; set; }
        public IList<MealItemContent> TableContents { get; set; }
        public bool AllowCreate { get; set; }

        public MealItemIndexModel(
            IEnumerable<MealItem> mealItems,
            bool hasElevatedPermissions = false)
        {
            MealItems = mealItems;
            HasElevatedPermissions = hasElevatedPermissions;
            TableContents = new List<MealItemContent>();
            foreach (var e in MealItems)
            {
                TableContents.Add(new MealItemContent(this, e));
            }
            TableContents = TableContents
                .OrderByDescending(e => e.VoteDifferential)
                .ThenBy(e => e.Entity.Name)
                .ToList();
            AllowCreate = true;
        }

        public class MealItemContent
        {
            private readonly MealItemIndexModel _parent;

            public MealItem Entity { get; set; }
            public bool AllowEdit { get; set; }
            public bool AllowDelete { get; set; }
            public int Upvotes { get; set; }
            public int Downvotes { get; set; }
            public int VoteDifferential { get; set; }

            public MealItemContent(MealItemIndexModel parent, MealItem entity)
            {
                _parent = parent;

                Entity = entity;
                AllowEdit = true;
                AllowDelete = !entity.Periods.Any();
                Upvotes = entity.Upvotes;
                Downvotes = entity.Downvotes;
                VoteDifferential = Upvotes - Downvotes;
            }
        }
    }
}