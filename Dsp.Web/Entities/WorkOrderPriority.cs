namespace Dsp.Web.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class WorkOrderPriority
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkOrderPriorityId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [InverseProperty("Priority")]
        public virtual ICollection<WorkOrderPriorityChange> PriorityChanges { get; set; }
    }
}
