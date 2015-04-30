namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WorkOrderPriorityChange
    {
        public int WorkOrderPriorityChangeId { get; set; }
        public int WorkOrderId { get; set; }
        public int WorkOrderPriorityId {get; set; }
        public int UserId { get; set; }

        public DateTime ChangedOn { get; set; }

        [ForeignKey("WorkOrderId")]
        public WorkOrder WorkOrder { get; set; }
        [ForeignKey("WorkOrderPriorityId")]
        public WorkOrderPriority Priority { get; set; }
        [ForeignKey("UserId")]
        public Member Member { get; set; }
    }
}
