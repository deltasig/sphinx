namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;

    public class WorkOrderPriority
    {
        public int WorkOrderPriorityId { get; set; }

        public string Name { get; set; }

        public ICollection<WorkOrderPriorityChange> PriorityChanges { get; set; }
    }
}
