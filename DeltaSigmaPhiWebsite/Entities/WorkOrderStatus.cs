namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("WorkOrderStatuses")]
    public class WorkOrderStatus
    {
        public int WorkOrderStatusId { get; set; }

        public string Name { get; set; }

        public ICollection<WorkOrderStatusChange> StatusChanges { get; set; } 
    }
}
