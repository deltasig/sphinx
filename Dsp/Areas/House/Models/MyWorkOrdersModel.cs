namespace Dsp.Areas.House.Models
{
    using Entities;
    using System.Collections.Generic;

    public class MyWorkOrdersModel
    {
        public IEnumerable<WorkOrder> CreatedWorkOrders { get; set; }
        public IEnumerable<WorkOrder> InvolvedWorkOrders { get; set; }
    }
}