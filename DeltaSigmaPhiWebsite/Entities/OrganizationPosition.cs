namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class OrganizationPosition
    {
        public OrganizationPosition()
        {
            OrganizationsJoineds = new HashSet<OrganizationsJoined>();
        }

        public int OrganizationPositionId { get; set; }

        public int OrganizationId { get; set; }

        [Required]
        [StringLength(50)]
        public string PositionName { get; set; }

        public virtual Organization Organization { get; set; }

        public virtual ICollection<OrganizationsJoined> OrganizationsJoineds { get; set; }
    }
}
