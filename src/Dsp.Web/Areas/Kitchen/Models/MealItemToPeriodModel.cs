namespace Dsp.Web.Areas.Kitchen.Models
{
    internal class MealItemToPeriodModel
    {
        public int Id { get; set; }
        public int MealItemId { get; set; }
        public int MealPeriodId { get; set; }
        public string MealItemName { get; set; }
    }
}