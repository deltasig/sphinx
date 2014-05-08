namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PledgeClass
    {
        public PledgeClass()
        {
            Members = new HashSet<Member>();
        }

        public int PledgeClassId { get; set; }

        public int SemesterId { get; set; }

        [Required]
        [StringLength(100)]
        public string PledgeClassName { get; set; }

        public virtual ICollection<Member> Members { get; set; }

        public virtual Semester Semester { get; set; }
    }
}
