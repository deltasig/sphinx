namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WorkOrderStatusChange
    {
        public int WorkOrderStatusChangeId { get; set; }
        public int WorkOrderId { get; set; }
        public int WorkOrderStatusId {get; set; }
        public int UserId { get; set; }

        public DateTime ChangedOn { get; set; }

        [ForeignKey("WorkOrderId")]
        public WorkOrder WorkOrder { get; set; }
        [ForeignKey("WorkOrderStatusId")]
        public WorkOrderStatus Status { get; set; }
        [ForeignKey("UserId")]
        public Member Member { get; set; }
    }
}
