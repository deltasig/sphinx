namespace Dsp.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class RoomToMember
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomToMemberId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime MovedIn { get; set; }

        public DateTime? MovedOut { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}