namespace Dsp.Web.Areas.Service.Models
{
    using System.Collections.Generic;

    public class ServiceHourIndexModel
    {
        public ServiceNavModel NavModel { get; }
        public IEnumerable<ServiceHourIndexMemberRowModel> ServiceHours { get; }

        public ServiceHourIndexModel(ServiceNavModel navModel, IEnumerable<ServiceHourIndexMemberRowModel> serviceHours)
        {
            NavModel = navModel;
            ServiceHours = serviceHours;
        }
    }
}
