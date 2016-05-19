namespace Dsp.Web.Areas.Kitchen.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class ReorderModel
    {
        public IList<object> MealToItems { get; set; }
    }
}