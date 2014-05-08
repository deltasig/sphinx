namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ChoreGroup
    {
        public ChoreGroup()
        {
            ChoreAssignments = new HashSet<ChoreAssignment>();
            Members = new HashSet<Member>();
        }

        public int ChoreGroupId { get; set; }

        public int SemesterId { get; set; }

        public int ChoreGroupTypeId { get; set; }

        public virtual ICollection<ChoreAssignment> ChoreAssignments { get; set; }

        public virtual ChoreGroupType ChoreGroupType { get; set; }

        public virtual Semester Semester { get; set; }

        public virtual ICollection<Member> Members { get; set; }
    }
}
