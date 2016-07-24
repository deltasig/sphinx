namespace Dsp.Data.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a completed challenge by a new member.
    /// </summary>
    public class QuestCompletion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Column(Order = 0), Index("IX_QuestCompletion", 0, IsUnique = true)]
        public int ChallengeId { get; set; }

        [Required, Column(Order = 1), Index("IX_QuestCompletion", 1, IsUnique = true)]
        public int NewMemberId { get; set; }

        public bool IsVerified { get; set; }

        [ForeignKey("ChallengeId")]
        public virtual QuestChallenge Challenge { get; set; }
        [ForeignKey("NewMemberId")]
        public virtual Member Member { get; set; }
    }
}
