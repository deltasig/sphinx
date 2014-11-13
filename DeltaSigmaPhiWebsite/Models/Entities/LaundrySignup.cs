namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("LaundrySignup")]
    public partial class LaundrySignup
    {
        [Key]
        public DateTime DateTimeShift { get; set; }

        public int UserId { get; set; }

        public DateTime DateTimeSignedUp { get; set; }

        public virtual Member Member { get; set; }
    }
}
