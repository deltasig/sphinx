namespace Dsp.WebCore.Areas.House.Models;

using Dsp.Data.Entities;
using System.Collections.Generic;

public class WorkOrderIndexModel
{
    public IEnumerable<WorkOrder> WorkOrders { get; set; }
    public IEnumerable<WorkOrder> UsersWorkOrders { get; set; }
    public WorkOrderIndexFilterModel Filter { get; set; }
    public int TotalPages { get; set; }
    public int OpenCount { get; set; }
    public int ClosedCount { get; set; }
    public int ResultCount { get; set; }

    public WorkOrderIndexModel(
        IEnumerable<WorkOrder> workOrders,
        IEnumerable<WorkOrder> usersWorkOrders,
        WorkOrderIndexFilterModel filter,
        int totalPages,
        int openCount,
        int closedCount)
    {
        WorkOrders = workOrders;
        UsersWorkOrders = usersWorkOrders;
        Filter = filter;
        TotalPages = totalPages;
        OpenCount = openCount;
        ClosedCount = closedCount;
        ResultCount = OpenCount + ClosedCount;
    }
}