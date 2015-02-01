using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DeltaSigmaPhiWebsite.Entities
{
    public partial class MealToPeriod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealToPeriodId { get; set; }

        public int MealPeriodId { get; set; }

        public int MealId { get; set; }

        [Column(TypeName="Date")]
        public DateTime Date { get; set; }

        [ForeignKey("MealPeriodId")]
        public virtual MealPeriod MealPeriod { get; set; }

        [ForeignKey("MealId")]
        public virtual Meal Meal { get; set; }
    }
}
