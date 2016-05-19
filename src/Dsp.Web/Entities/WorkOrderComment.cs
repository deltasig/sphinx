namespace Dsp.Web.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WorkOrderComment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkOrderCommentId { get; set; }

        public int WorkOrderId { get; set; }

        public int? UserId { get; set; }

        public DateTime SubmittedOn { get; set; }

        [Required, StringLength(3000)]
        public string Text { get; set; }

        [ForeignKey("WorkOrderId")]
        public virtual WorkOrder WorkOrder { get; set; }
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}
