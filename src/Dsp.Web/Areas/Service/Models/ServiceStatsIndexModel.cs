using Dsp.Services.Models;
using System.Collections.Generic;

namespace Dsp.Web.Areas.Service.Models
{
    public class ServiceStatsIndexModel
    {
        public ServiceNavModel NavModel { get; }
        public ServiceMemberStats SemesterMemberStats { get; }
        public IEnumerable<ServiceGeneralHistoricalStats> GeneralStats { get; }

        public ServiceStatsIndexModel(
            ServiceNavModel navModel,
            ServiceMemberStats semesterMemberStats,
            IEnumerable<ServiceGeneralHistoricalStats> generalStats)
        {
            NavModel = navModel;
            SemesterMemberStats = semesterMemberStats;
            GeneralStats = generalStats;
        }
    }
}
