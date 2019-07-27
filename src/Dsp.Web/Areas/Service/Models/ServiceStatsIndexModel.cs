namespace Dsp.Web.Areas.Service.Models
{
    public class ServiceStatsIndexModel
    {
        public ServiceNavModel NavModel { get; }

        public ServiceStatsIndexModel(ServiceNavModel navModel)
        {
            NavModel = navModel;
        }
    }
}
