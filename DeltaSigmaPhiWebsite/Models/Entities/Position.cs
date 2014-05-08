namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Position
    {
        public Position()
        {
            Leaders = new HashSet<Leader>();
        }

        public int PositionId { get; set; }

        [StringLength(50)]
        public string PositionName { get; set; }

        public bool? IsExecutive { get; set; }

        public bool? IsElected { get; set; }

        public virtual ICollection<Leader> Leaders { get; set; }
    }
}
