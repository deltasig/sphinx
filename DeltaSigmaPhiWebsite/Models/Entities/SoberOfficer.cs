namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SoberOfficer
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
