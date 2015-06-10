namespace Dsp.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class LaundrySignup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DateTime DateTimeShift { get; set; }

        public int UserId { get; set; }

        public DateTime DateTimeSignedUp { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}
