namespace Dsp.Web.Areas.House.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class WorkOrderIndexModel
    {
        public IEnumerable<WorkOrder> WorkOrders { get; set; }
        public MyWorkOrdersModel UserWorkOrders { get; set; }
    }
}