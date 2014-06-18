namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class PledgeClass
    {
        public PledgeClass()
        {
            Members = new HashSet<Member>();
        }

        public int PledgeClassId { get; set; }

        public int SemesterId { get; set; }

        public DateTime? PinningDate { get; set; }

        public DateTime? InitiationDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PledgeClassName { get; set; }

        public virtual ICollection<Member> Members { get; set; }

        public virtual Semester Semester { get; set; }
    }
}
