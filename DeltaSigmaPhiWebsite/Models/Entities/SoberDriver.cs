namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class SoberDriver
    {
        [Key]
        public int SignupId { get; set; }

        public int? UserId { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateOfShift { get; set; }

        public DateTime DateTimeSignedUp { get; set; }

        public virtual Member Member { get; set; }
    }
}
