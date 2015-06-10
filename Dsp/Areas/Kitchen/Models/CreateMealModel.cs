namespace Dsp.Areas.Kitchen.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class CreateMealModel
    {
        public int[] SelectedMealItemIds { get; set; }
        public IEnumerable<SelectListItem> MealItems { get; set; }
    }
}