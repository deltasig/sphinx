namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Leader
    {
        public Leader()
        {
            Members = new HashSet<Member>();
        }

        public int LeaderId { get; set; }

        public int UserId { get; set; }

        public int PositionId { get; set; }

        public int SemesterId { get; set; }

        public double RemainingBudgetBalance { get; set; }

        public virtual Position Position { get; set; }

        public virtual Semester Semester { get; set; }

        public virtual Member Member { get; set; }

        public virtual ICollection<Member> Members { get; set; }
    }
}
