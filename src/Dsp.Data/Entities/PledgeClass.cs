namespace Dsp.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

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

        public string GetLetters()
        {
            var splits = PledgeClassName.Split(' ');
            var isTrueAlpha = splits.Contains("Alpha") && (splits.Contains("1") || splits.Contains("2") || splits.Contains("3"));
            if (isTrueAlpha) return "&Alpha;";

            return string.Join("", splits.Select(m => "&" + m + ";"));
        }
    }
}
