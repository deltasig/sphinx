namespace Dsp.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MealsToPeriods")]
    public class MealToPeriod
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MealPeriodId { get; set; }

        [Required]
        public int MealId { get; set; }

        [Required, Column(TypeName = "Date")]
        public DateTime Date { get; set; }

        [ForeignKey("MealPeriodId")]
        public virtual MealPeriod MealPeriod { get; set; }
        [ForeignKey("MealId")]
        public virtual Meal Meal { get; set; }
    }
}
