namespace Dsp.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ChorePeriod
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Begins On")]
        public DateTime BeginsOnCst { get; set; }

        [Display(Name = "Ends On")]
        public DateTime EndsOnCst { get; set; }

        [InverseProperty("Period")]
        public virtual ICollection<ChoreAssignment> Assignments { get; set; }
    }
}
