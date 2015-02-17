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
        [Required]
        [Display(Name = "Major")]
        public int MajorId { get; set; }

        [Column(Order = 1)]
        [Index("IX_MajorToMember", 1, IsUnique = true)]
        [Required]
        [Display(Name = "Member")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Degree Level")]
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
