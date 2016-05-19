namespace Dsp.Web.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WorkOrderStatusChange
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkOrderStatusChangeId { get; set; }

        public int WorkOrderId { get; set; }

        public int WorkOrderStatusId {get; set; }

        public int? UserId { get; set; }

        public DateTime ChangedOn { get; set; }

        [ForeignKey("WorkOrderId")]
        public virtual WorkOrder WorkOrder { get; set; }
        [ForeignKey("WorkOrderStatusId")]
        public virtual WorkOrderStatus Status { get; set; }
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}
