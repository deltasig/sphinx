namespace Dsp.Data.Entities
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

        public DateTime MovedOut { get; set; }

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }

        public string GetSemester()
        {
            if (MovedIn.Month < 6) return "Spring " + MovedIn.Year;
            else if (MovedIn.Month < 8) return "Summer " + MovedIn.Year;
            else return "Fall " + MovedIn.Year;
        }
    }
}