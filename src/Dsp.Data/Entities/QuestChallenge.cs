namespace Dsp.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a challenge put forth by an active member to any available new member.
    /// </summary>
    public class QuestChallenge
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Column(Order = 0), Index("IX_QuestChallenge", 0, IsUnique = true)]
        public int SemesterId { get; set; }

        [Required, Column(Order = 1), Index("IX_QuestChallenge", 1, IsUnique = true)]
        public int MemberId { get; set; }

        [Required, Column(Order = 2), Index("IX_QuestChallenge", 2, IsUnique = true)]
        [Display(Name = "Begins On")]
        public DateTime? BeginsOn { get; set; }

        [Required, Column(Order = 3), Index("IX_QuestChallenge", 3, IsUnique = true)]
        [Display(Name = "Ends On")]
        public DateTime? EndsOn { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }
        [InverseProperty("Challenge")]
        public virtual ICollection<QuestCompletion> Completions { get; set; }
    }
}
