namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Leader
    {
        public Leader()
        {
            Members = new HashSet<Member>();
        }

        public int LeaderId { get; set; }

        public int UserId { get; set; }

        public int PositionId { get; set; }

        public DateTime AppointedOn { get; set; }

        public int? SemesterId { get; set; }

        public double? RemainingBudgetBalance { get; set; }

        public virtual Position Position { get; set; }

        public virtual Semester Semester { get; set; }

        public virtual Member Member { get; set; }

        public virtual ICollection<Member> Members { get; set; }
    }
}
