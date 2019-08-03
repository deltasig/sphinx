namespace Dsp.Web.Areas.Service.Models
{
    using Dsp.Services.Models;
    using System.Collections.Generic;

    public class ServiceHourIndexModel
    {
        public ServiceNavModel NavModel { get; }
        public IEnumerable<ServiceMemberProgress> Progress { get; }

        public ServiceHourIndexModel(ServiceNavModel navModel, IEnumerable<ServiceMemberProgress> progress)
        {
            NavModel = navModel;
            Progress = progress;
        }
    }
}
