namespace Dsp.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("WorkOrderStatuses")]
    public class WorkOrderStatus
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkOrderStatusId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [InverseProperty("Status")]
        public virtual ICollection<WorkOrderStatusChange> StatusChanges { get; set; } 
    }
}
