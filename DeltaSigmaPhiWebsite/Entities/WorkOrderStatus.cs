namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("WorkOrderStatuses")]
    public class WorkOrderStatus
    {
        public int WorkOrderStatusId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<WorkOrderStatusChange> StatusChanges { get; set; } 
    }
}
