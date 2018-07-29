namespace Dsp.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MealItemsToPeriods")]
    public class MealItemToPeriod
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int MealPeriodId { get; set; }

        [Required]
        public int MealItemId { get; set; }

        [Required, Column(TypeName = "Date")]
        public DateTime Date { get; set; }

        [ForeignKey("MealPeriodId")]
        public virtual MealPeriod MealPeriod { get; set; }
        [ForeignKey("MealItemId")]
        public virtual MealItem MealItem { get; set; }
    }
}
