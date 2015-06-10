namespace Dsp.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WorkOrderPriorityChange
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkOrderPriorityChangeId { get; set; }

        public int WorkOrderId { get; set; }

        public int WorkOrderPriorityId {get; set; }

        public int? UserId { get; set; }

        public DateTime ChangedOn { get; set; }

        [ForeignKey("WorkOrderId")]
        public virtual WorkOrder WorkOrder { get; set; }
        [ForeignKey("WorkOrderPriorityId")]
        public virtual WorkOrderPriority Priority { get; set; }
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}
