namespace Dsp.Services
{
    using Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Interfaces;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ServiceService : BaseService, IServiceService
    {
        private readonly IRepository _repository;

        public ServiceService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public ServiceService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public ServiceService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ServiceEvent>> GetEventsForSemesterAsync(Semester semester)
        {
            var events = await _repository.GetAsync<ServiceEvent>(
                filter: x => x.SemesterId == semester.SemesterId,
                orderBy: x => x.OrderByDescending(o => o.DateTimeOccurred)
            );

            return events;
        }

        public async Task<IEnumerable<Semester>> GetSemestersWithEventsAsync(Semester currentSemester, bool includeCurrent = true)
        {
            var semesters = await _repository.GetAsync<Semester>(
                filter: x => x.ServiceEvents.Any(),
                orderBy: x => x.OrderByDescending(o => o.DateStart));

            if (includeCurrent && !semesters.Any(x => x.SemesterId == currentSemester.SemesterId))
            {
                semesters = semesters
                    .Concat(new List<Semester> { currentSemester })
                    .OrderByDescending(x => x.DateStart);
            }

            return semesters;
        }

        public async Task<ServiceEvent> GetEventByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<ServiceEvent>(id);
        }

        public async Task<ServiceHour> GetHoursAsync(int eventId, int userId)
        {
            return await _repository.GetOneAsync<ServiceHour>(filter: x => x.EventId == eventId && x.UserId == userId);
        }

        public async Task<IEnumerable<ServiceEventAmendment>> GetEventAmendmentsBySemesterIdAsync(int semesterId)
        {
            return await _repository.GetAsync<ServiceEventAmendment>(filter: x => x.SemesterId == semesterId);
        }

        public async Task<IEnumerable<ServiceHourAmendment>> GetHoursAmendmentsBySemesterIdAsync(int semesterId)
        {
            return await _repository.GetAsync<ServiceHourAmendment>(filter: x => x.SemesterId == semesterId);
        }

        public async Task<ServiceEventAmendment> GetEventAmendmentByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<ServiceEventAmendment>(id);
        }

        public async Task<ServiceHourAmendment> GetHoursAmendmentByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<ServiceHourAmendment>(id);
        }

        public async Task CreateEventAsync(ServiceEvent serviceEvent)
        {
            _repository.Create(serviceEvent);
            await _repository.SaveAsync();
        }

        public async Task CreateHoursAsync(ServiceHour serviceHours)
        {
            _repository.Create(serviceHours);
            await _repository.SaveAsync();
        }

        public async Task CreateEventAmendmentAsync(ServiceEventAmendment eventAmendment)
        {
            _repository.Create(eventAmendment);
            await _repository.SaveAsync();
        }

        public async Task CreateHoursAmendmentAsync(ServiceHourAmendment hoursAmendment)
        {
            _repository.Create(hoursAmendment);
            await _repository.SaveAsync();
        }

        public async Task UpdateEventAsync(ServiceEvent serviceEvent)
        {
            _repository.Update(serviceEvent);
            await _repository.SaveAsync();
        }

        public async Task UpdateHoursAsync(ServiceHour serviceHours)
        {
            _repository.Update(serviceHours);
            await _repository.SaveAsync();
        }

        public async Task DeleteEventByIdAsync(int id)
        {
            _repository.Delete<ServiceEvent>(id);
            await _repository.SaveAsync();
        }

        public async Task DeleteHoursAsync(ServiceHour serviceHours)
        {
            _repository.Delete(serviceHours);
            await _repository.SaveAsync();
        }

        public async Task DeleteEventAmendmentByIdAsync(int id)
        {
            _repository.Delete<ServiceEventAmendment>(id);
            await _repository.SaveAsync();
        }

        public async Task DeleteHoursAmendmentByIdAsync(int id)
        {
            _repository.Delete<ServiceHourAmendment>(id);
            await _repository.SaveAsync();
        }
    }
}
