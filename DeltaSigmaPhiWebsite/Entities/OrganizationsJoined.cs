namespace DeltaSigmaPhiWebsite.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("OrganizationsJoined")]
    public partial class OrganizationsJoined
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OrganizationId { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SemesterId { get; set; }

        public int? OrganizationPositionId { get; set; }

        public virtual Member Member { get; set; }

        public virtual OrganizationPosition OrganizationPosition { get; set; }

        public virtual Organization Organization { get; set; }

        public virtual Semester Semester { get; set; }
    }
}
