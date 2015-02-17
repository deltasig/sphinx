namespace DeltaSigmaPhiWebsite.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MajorToMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MajorToMemberId { get; set; }

        [Column(Order = 0)]
        [Index("IX_MajorToMember", 0, IsUnique = true)]
        public int MajorId { get; set; }

        [Column(Order = 1)]
        [Index("IX_MajorToMember", 1, IsUnique = true)]
        public int UserId { get; set; }

        public DegreeLevel DegreeLevel { get; set; }

        [ForeignKey("MajorId")]
        public virtual Major Major { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }

    public enum DegreeLevel
    {
        Bs,
        Minor,
        Ms,
        PhD,
        Cert
    }
}
