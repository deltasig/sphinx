namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class WorkOrderPriority
    {
        public int WorkOrderPriorityId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<WorkOrderPriorityChange> PriorityChanges { get; set; }
    }
}
