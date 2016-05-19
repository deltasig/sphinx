namespace Dsp.Data.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Room
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomId { get; set; }

        [Required]
        [Column(Order = 0), Index("IX_RoomSemesterName", 0, IsUnique = true)]
        public int SemesterId { get; set; }

        [Required, StringLength(100)]
        [Column(Order = 1), Index("IX_RoomSemesterName", 1, IsUnique = true)]
        public string Name { get; set; }

        [Required, Range(0, 10), Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }
        [InverseProperty("Room")]
        public virtual ICollection<RoomToMember> Members { get; set; }
    }
}