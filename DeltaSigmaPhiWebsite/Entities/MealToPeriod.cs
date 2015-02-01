﻿namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MealToPeriod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealToPeriodId { get; set; }

        public int MealPeriodId { get; set; }

        public int MealId { get; set; }

        [Required]
        [Column(TypeName="Date")]
        public DateTime Date { get; set; }

        [ForeignKey("MealPeriodId")]
        public virtual MealPeriod MealPeriod { get; set; }

        [ForeignKey("MealId")]
        public virtual Meal Meal { get; set; }
    }
}
