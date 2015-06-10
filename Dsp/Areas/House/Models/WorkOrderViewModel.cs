namespace Dsp.Areas.House.Models
{
    using Entities;

    public class WorkOrderViewModel
    {
        public WorkOrder WorkOrder { get; set; }
        public MyWorkOrdersModel UserWorkOrders { get; set; }
    }
}