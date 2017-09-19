namespace Dsp.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MealToPeriod
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealToPeriodId { get; set; }

        [Required, Column(Order = 0), Index("IX_MealToPeriod", 0, IsUnique = true)]
        public int MealPeriodId { get; set; }

        [Required, Column(Order = 1), Index("IX_MealToPeriod", 1, IsUnique = true)]
        public int MealId { get; set; }

        [Required, Column(Order = 2, TypeName="Date"), Index("IX_MealToPeriod", 2, IsUnique = true)]
        public DateTime Date { get; set; }

        [ForeignKey("MealPeriodId")]
        public virtual MealPeriod MealPeriod { get; set; }
        [ForeignKey("MealId")]
        public virtual Meal Meal { get; set; }
    }
}
