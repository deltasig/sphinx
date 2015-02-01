namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Meal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealId { get; set; }

        public virtual ICollection<MealToItem> MealsToItems { get; set; }

        public virtual ICollection<MealToPeriod> MealsToPeriods { get; set; }

        public override string ToString()
        {
            if (MealsToItems == null) return "No Meal Items";
            var label = string.Empty;
            foreach (var m in MealsToItems)
            {
                label += m.MealItem.Name + ", ";
            }
            return label.Substring(0, label.Length - 2);
        }
    }
}
