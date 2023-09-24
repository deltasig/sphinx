namespace Dsp.WebCore.Areas.Service.Models;

using Dsp.Data.Entities;
using System.Collections.Generic;

public class ServiceAmendmentModel
{
    public ServiceNavModel NavModel { get; }
    public IEnumerable<ServiceHourAmendment> ServiceHourAmendments { get; }
    public IEnumerable<ServiceEventAmendment> ServiceEventAmendments { get; }

    public ServiceAmendmentModel(
        ServiceNavModel navModel,
        IEnumerable<ServiceHourAmendment> serviceHourAmendments,
        IEnumerable<ServiceEventAmendment> serviceEventAmendments)
    {
        NavModel = navModel;
        ServiceHourAmendments = serviceHourAmendments;
        ServiceEventAmendments = serviceEventAmendments;
    }
}
