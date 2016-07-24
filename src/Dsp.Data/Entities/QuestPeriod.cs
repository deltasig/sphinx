namespace Dsp.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a time span during which broquesting occurs.
    /// </summary>
    public class QuestPeriod
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Begins On")]
        public DateTime BeginsOn { get; set; }

        [Display(Name = "Ends On")]
        public DateTime EndsOn { get; set; }

        [InverseProperty("Period")]
        public virtual ICollection<QuestChallenge> Challenges { get; set; }
    }
}
