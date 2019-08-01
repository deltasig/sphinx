using Dsp.Data.Entities;
using Dsp.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dsp.Services.Interfaces
{
    public interface IServiceService : IService
    {
        Task<IEnumerable<ServiceEvent>> GetEventsForSemesterAsync(Semester semester);
        Task<IEnumerable<Semester>> GetSemestersWithEventsAsync(Semester currentSemester, bool includeCurrent = true);
        Task<ServiceEvent> GetEventByIdAsync(int id);
        Task<ServiceHour> GetHoursAsync(int eventId, int userId);
        Task<IEnumerable<ServiceEventAmendment>> GetEventAmendmentsBySemesterIdAsync(int semesterId);
        Task<IEnumerable<ServiceHourAmendment>> GetHoursAmendmentsBySemesterIdAsync(int semesterId);
        Task<ServiceEventAmendment> GetEventAmendmentByIdAsync(int id);
        Task<ServiceHourAmendment> GetHoursAmendmentByIdAsync(int id);

        Task CreateEventAsync(ServiceEvent serviceEvent);
        Task CreateHoursAsync(ServiceHour serviceHours);
        Task CreateEventAmendmentAsync(ServiceEventAmendment eventAmendment);
        Task CreateHoursAmendmentAsync(ServiceHourAmendment hoursAmendment);

        Task UpdateEventAsync(ServiceEvent serviceEvent);
        Task UpdateHoursAsync(ServiceHour serviceHours);

        Task DeleteEventByIdAsync(int id);
        Task DeleteHoursAsync(ServiceHour serviceHours);
        Task DeleteEventAmendmentByIdAsync(int id);
        Task DeleteHoursAmendmentByIdAsync(int id);

        Task<ServiceMemberStats> GetMemberStatsBySemesterIdAsync(int sid);
        Task<ServiceHourStats> GetHourStatsBySemesterIdAsync(int sid);
    }
}
