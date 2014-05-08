namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Major
    {
        public Major()
        {
            Members = new HashSet<Member>();
        }

        public int MajorId { get; set; }

        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string MajorName { get; set; }

        public virtual Department Department { get; set; }

        public virtual ICollection<Member> Members { get; set; }
    }
}
