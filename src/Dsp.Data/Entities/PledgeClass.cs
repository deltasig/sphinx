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

        [DataType(DataType.Date), Display(Name = "Pinning Date")]
        public DateTime? PinningDate { get; set; }

        [DataType(DataType.Date), Display(Name = "Initiation Date")]
        public DateTime? InitiationDate { get; set; }

        [Required, StringLength(50), Display(Name = "Name")]
        public string PledgeClassName { get; set; }

        [InverseProperty("PledgeClass")] 
        public virtual ICollection<Member> Members { get; set; }
        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }
    }
}
