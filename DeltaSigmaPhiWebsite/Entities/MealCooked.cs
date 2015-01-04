namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MealsCooked")]
    public partial class MealCooked
    {
        [Key]
        public int ServingId { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateServed { get; set; }

        public int MealId { get; set; }

        public bool? Lunch { get; set; }

        public bool? Dinner { get; set; }

        public virtual Meal Meal { get; set; }
    }
}
