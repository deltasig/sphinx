namespace Dsp.Data.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MealsToItems")]
    public class MealToItem
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MealId { get; set; }

        public int MealItemId { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey("MealId")]
        public virtual Meal Meal { get; set; }
        [ForeignKey("MealItemId")]
        public virtual MealItem MealItem { get; set; }
    }
}
