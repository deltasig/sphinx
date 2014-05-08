namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Meal
    {
        public Meal()
        {
            MealsCooked = new HashSet<MealCooked>();
        }

        public int MealId { get; set; }

        [Required]
        [StringLength(100)]
        public string MealTitle { get; set; }

        public virtual ICollection<MealCooked> MealsCooked { get; set; }
    }
}
