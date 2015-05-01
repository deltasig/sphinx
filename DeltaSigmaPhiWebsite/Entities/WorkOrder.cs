﻿namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WorkOrder
    {
        public int WorkOrderId { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(3000)]
        public string Description { get; set; }
        [DataType(DataType.MultilineText)]
        [StringLength(3000)]
        public string Result { get; set; }

        public virtual ICollection<WorkOrderStatusChange> StatusChanges { get; set; }
        public virtual ICollection<WorkOrderPriorityChange> PriorityChanges { get; set; }
        public virtual ICollection<WorkOrderComment> Comments { get; set; }
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}