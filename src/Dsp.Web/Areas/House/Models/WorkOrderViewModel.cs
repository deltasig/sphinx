namespace Dsp.Web.Areas.House.Models
{
    using Dsp.Data.Entities;

    public class WorkOrderViewModel
    {
        public WorkOrder WorkOrder { get; set; }
        public MyWorkOrdersModel UserWorkOrders { get; set; }
    }
}