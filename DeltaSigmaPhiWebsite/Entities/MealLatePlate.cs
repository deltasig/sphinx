namespace DeltaSigmaPhiWebsite.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MealLatePlate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealLatePlateId { get; set; }

        [Column(Order = 0)]
        [Index("IX_MealLatePlate", 0, IsUnique = true)]
        public int UserId { get; set; }

        [Column(Order = 1)]
        [Index("IX_MealLatePlate", 1, IsUnique = true)]
        public int MealToPeriodId { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }

        [ForeignKey("MealToPeriodId")]
        public virtual MealToPeriod MealToPeriod { get; set; }
    }
}
