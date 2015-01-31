using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DeltaSigmaPhiWebsite.Areas.Kitchen.Models
{
    public class CreateMealModel
    {
        public int[] SelectedMealItemIds { get; set; }
        public IEnumerable<SelectListItem> MealItems { get; set; }
    }
}