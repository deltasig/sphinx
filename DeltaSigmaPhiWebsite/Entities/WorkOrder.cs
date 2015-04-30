namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WorkOrder
    {
        public int WorkOrderId { get; set; }
        public int UserId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }

        public ICollection<WorkOrderStatusChange> StatusChanges { get; set; }
        public ICollection<WorkOrderPriorityChange> PriorityChanges { get; set; }
        public ICollection<WorkOrderComment> Comments { get; set; }
        [ForeignKey("UserId")]
        public Member Member { get; set; }
    }
}