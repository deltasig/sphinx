namespace Dsp.Web.Areas.House.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class WorkOrderViewModel
    {
        public WorkOrder WorkOrder { get; set; }
        public IEnumerable<WorkOrder> UsersWorkOrders { get; set; }
    }
}