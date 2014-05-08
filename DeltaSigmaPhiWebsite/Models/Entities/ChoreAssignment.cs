namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ChoreAssignment
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ChoreId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ChoreGroupId { get; set; }

        public DateTime DateTimeIssued { get; set; }

        public DateTime? DateTimeCompleted { get; set; }

        public bool IsSatisfactory { get; set; }

        public virtual ChoreGroup ChoreGroup { get; set; }

        public virtual Chore Chore { get; set; }
    }
}
