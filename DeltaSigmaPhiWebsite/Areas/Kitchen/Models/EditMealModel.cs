namespace DeltaSigmaPhiWebsite.Areas.Kitchen.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class EditMealModel
    {
        public int MealId { get; set; }
        public int[] SelectedMealItemIds { get; set; }
        public IEnumerable<SelectListItem> MealItems { get; set; }
    }
}