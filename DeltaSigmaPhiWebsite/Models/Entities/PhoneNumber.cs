namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PhoneNumber
    {
        public int PhoneNumberId { get; set; }

        public int UserId { get; set; }

        [Column("PhoneNumber")]
        public int PhoneNumber1 { get; set; }

        public virtual Member Member { get; set; }
    }
}
