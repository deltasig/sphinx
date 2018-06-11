namespace Dsp.Data.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class Meal
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [InverseProperty("Meal")]
        public virtual ICollection<MealToItem> MealItems { get; set; }
        [InverseProperty("Meal")]
        public virtual ICollection<MealToPeriod> MealPeriods { get; set; }

        public override string ToString()
        {
            if (MealItems == null) return "No Meal Items";
            var label = string.Empty;
            foreach (var m in MealItems.OrderBy(i => i.DisplayOrder))
            {
                label += m.MealItem.Name + ", ";
            }
            return label.Substring(0, label.Length - 2);
        }
    }
}
