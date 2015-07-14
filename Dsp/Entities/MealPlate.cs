namespace Dsp.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MealPlate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealPlateId { get; set; }

        public int UserId { get; set; }

        public DateTime PlateDateTime { get; set; }

        public DateTime SignedUpOn { get; set; }

        [StringLength(25)]
        public string Type { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}
