namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Organization
    {
        public Organization()
        {
            OrganizationPositions = new HashSet<OrganizationPosition>();
            OrganizationsJoineds = new HashSet<OrganizationsJoined>();
        }

        public int OrganizationId { get; set; }

        [Required]
        [StringLength(100)]
        public string OrganizationName { get; set; }

        public virtual ICollection<OrganizationPosition> OrganizationPositions { get; set; }

        public virtual ICollection<OrganizationsJoined> OrganizationsJoineds { get; set; }
    }
}
