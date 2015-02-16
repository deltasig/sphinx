namespace DeltaSigmaPhiWebsite.Areas.Kitchen.Models
{
    using Entities;
    using System.Collections.Generic;

    public class ReorderModel
    {
        public IList<object> MealToItems { get; set; }
    }
}