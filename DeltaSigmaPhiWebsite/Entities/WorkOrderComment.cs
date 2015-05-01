namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WorkOrderComment
    {
        public int WorkOrderCommentId { get; set; }
        public int WorkOrderId { get; set; }
        public int UserId { get; set; }

        public DateTime SubmittedOn { get; set; }
        [Required]
        [StringLength(3000)]
        public string Text { get; set; }

        [ForeignKey("WorkOrderId")]
        public WorkOrder WorkOrder { get; set; }
        [ForeignKey("UserId")]
        public Member Member { get; set; }
    }
}
