namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ChoreGroupType
    {
        public ChoreGroupType()
        {
            ChoreGroups = new HashSet<ChoreGroup>();
        }

        public int ChoreGroupTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string ChoreType { get; set; }

        public virtual ICollection<ChoreGroup> ChoreGroups { get; set; }
    }
}
