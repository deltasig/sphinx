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
        
        public int SemesterId { get; set; }
        
        public int MemberId { get; set; }

        [Display(Name = "Begins On")]
        public DateTime? BeginsOn { get; set; }

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
