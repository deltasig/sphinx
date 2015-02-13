namespace DeltaSigmaPhiWebsite.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ClassFileVote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClassFileVoteId { get; set; }

        [Column(Order = 0)]
        [Index("IX_ClassFileVote", 0, IsUnique = true)]
        public int UserId { get; set; }

        [Column(Order = 1)]
        [Index("IX_ClassFileVote", 1, IsUnique = true)]
        public int ClassFileId { get; set; }

        public bool IsUpvote { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }

        [ForeignKey("ClassFileId")]
        public virtual ClassFile ClassFile { get; set; }
    }
}