namespace Dsp.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PledgeClass
    {
        public int PledgeClassId { get; set; }

        public int SemesterId { get; set; }

        public DateTime? PinningDate { get; set; }

        public DateTime? InitiationDate { get; set; }

        [Required, StringLength(50)]
        public string PledgeClassName { get; set; }

        [InverseProperty("PledgeClass")] 
        public virtual ICollection<Member> Members { get; set; }
        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }
    }
}
