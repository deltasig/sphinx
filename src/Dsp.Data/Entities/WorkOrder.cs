namespace Dsp.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class WorkOrder
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkOrderId { get; set; }

        public int? UserId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required, DataType(DataType.MultilineText), StringLength(3000)]
        public string Description { get; set; }

        [DataType(DataType.MultilineText), StringLength(3000)]
        public string Result { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
        [InverseProperty("WorkOrder")]
        public virtual ICollection<WorkOrderStatusChange> StatusChanges { get; set; }
        [InverseProperty("WorkOrder")]
        public virtual ICollection<WorkOrderPriorityChange> PriorityChanges { get; set; }
        [InverseProperty("WorkOrder")]
        public virtual ICollection<WorkOrderComment> Comments { get; set; }

        public string GetCurrentStatus()
        {
            return StatusChanges.OrderBy(o => o.ChangedOn).Last().Status.Name;
        }
        public string GetCurrentPriority()
        {
            return PriorityChanges.OrderBy(o => o.ChangedOn).Last().Priority.Name;
        }
        public DateTime GetDateTimeCreated()
        {
            return StatusChanges.OrderBy(o => o.ChangedOn).First().ChangedOn;
        }
        public DateTime GetMostRecentActivityDateTime()
        {
            var mostRecentComment = new DateTime();
            if (Comments.Any())
            {
                mostRecentComment = Comments.Max(w => w.SubmittedOn);
            }

            var dates = new List<DateTime>
            {
                StatusChanges.Max(w => w.ChangedOn), 
                PriorityChanges.Max(w => w.ChangedOn), 
                mostRecentComment
            };

            return dates.Max();
        }
    }
}