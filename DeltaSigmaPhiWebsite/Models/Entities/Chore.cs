namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Chore
    {
        public Chore()
        {
            ChoreAssignments = new HashSet<ChoreAssignment>();
        }

        public int ChoreId { get; set; }

        public int? ChoreClassId { get; set; }

        [Required]
        [StringLength(50)]
        public string ChoreName { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        public string Directions { get; set; }

        public bool? OnSunday { get; set; }

        public bool? OnMonday { get; set; }

        public bool? OnTuesday { get; set; }

        public bool? OnWednesday { get; set; }

        public bool? OnThursday { get; set; }

        public bool? OnFriday { get; set; }

        public bool? OnSaturday { get; set; }

        public virtual ICollection<ChoreAssignment> ChoreAssignments { get; set; }

        public virtual ChoreClass ChoreClass { get; set; }
    }
}
