namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("LaundrySignup")]
    public partial class LaundrySignup
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime DateTimeShift { get; set; }

        public DateTime DateTimeSignedUp { get; set; }

        public virtual Member Member { get; set; }
    }
}
