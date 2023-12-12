namespace Dsp.WebCore.Areas.Service.Models;

using Dsp.Data.Entities;
using System.Collections.Generic;

public class ServiceEventIndexModel
{
    public ServiceNavModel NavModel { get; }
    public IEnumerable<ServiceEvent> Events { get; }

    public ServiceEventIndexModel(ServiceNavModel navModel, IEnumerable<ServiceEvent> serviceEvents)
    {
        NavModel = navModel;
        Events = serviceEvents;
    }
}
