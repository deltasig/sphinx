namespace Dsp.Data.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MajorToMember
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MajorToMemberId { get; set; }

        [Required, Column(Order = 0), Index("IX_MajorToMember", 0, IsUnique = true)]
        [Display(Name = "Major")]
        public int MajorId { get; set; }

        [Required, Column(Order = 1), Index("IX_MajorToMember", 1, IsUnique = true)]
        [Display(Name = "Member")]
        public int UserId { get; set; }

        [Required, Column(Order = 2), Index("IX_MajorToMember", 2, IsUnique = true)]
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
        Cert,
        Ba
    }
}
