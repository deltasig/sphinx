using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dsp.Data.Entities;

public partial class WorkOrder
{
    public int WorkOrderId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string Result { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ClosedOn { get; set; }

    public virtual User User { get; set; }

    public bool IsClosed
    {
        get { return ClosedOn != null; }
    }

    public bool IsOpen
    {
        get { return ClosedOn == null; }
    }
}
