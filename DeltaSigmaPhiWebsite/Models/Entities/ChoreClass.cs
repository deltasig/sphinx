namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ChoreClass
    {
        public ChoreClass()
        {
            Chores = new HashSet<Chore>();
        }

        public int ChoreClassId { get; set; }

        [Required]
        [StringLength(50)]
        public string ChoreClassName { get; set; }

        public virtual ICollection<Chore> Chores { get; set; }
    }
}
