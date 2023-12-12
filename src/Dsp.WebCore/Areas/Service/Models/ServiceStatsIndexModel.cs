namespace Dsp.WebCore.Areas.Service.Models;

using Dsp.Services.Models;

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
