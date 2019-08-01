namespace Dsp.Services
{
    using Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Dsp.Services.Models;
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading.Tasks;

    public class ServiceService : BaseService, IServiceService
    {
        private readonly IRepository _repository;
        private readonly IMemberService _memberService;
        private readonly ISemesterService _semesterService;

        public ServiceService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public ServiceService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public ServiceService(IRepository repository)
        {
            _repository = repository;
            _memberService = new MemberService(repository);
            _semesterService = new SemesterService(repository);
        }


        public async Task<IEnumerable<ServiceEvent>> GetEventsBySemesterIdAsync(int sid)
        {
            var events = await _repository.GetAsync<ServiceEvent>(
                filter: x => x.SemesterId == sid,
                orderBy: x => x.OrderByDescending(o => o.DateTimeOccurred)
            );

            return events;
        }

        public async Task<IEnumerable<ServiceEvent>> GetEventsForSemesterAsync(Semester semester)
        {
            var events = await GetEventsBySemesterIdAsync(semester.SemesterId);

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


        public async Task<ServiceMemberStats> GetMemberStatsBySemesterIdAsync(int sid)
        {
            // Check the cache
            ObjectCache cache = MemoryCache.Default;
            if (cache[sid.ToString()] is ServiceMemberStats model) return model;

            // Calculate stats
            var semester = await _semesterService.GetSemesterByIdAsync(sid);
            var roster = await _memberService.GetRosterForSemesterAsync(sid);








            model = new ServiceMemberStats();
            // Add model to cache
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(6)
            };
            cache.Set(sid.ToString(), model, policy);

            return model;
        }

        public async Task<ServiceHourStats> GetHourStatsBySemesterIdAsync(int sid)
        {
            // Check the cache
            ObjectCache cache = MemoryCache.Default;
            if (cache[sid.ToString()] is ServiceHourStats model) return model;

            // Calculate stats
            var selectedSemester = await _semesterService.GetSemesterByIdAsync(sid);
            var priorSemester = await _semesterService.GetPriorSemesterAsync(selectedSemester);
            var roster = await _memberService.GetRosterForSemesterAsync(sid);
            var unadjustedMemberCount = roster.Count();
            roster = roster.Where(x => x.ServiceHourAmendments
                    .Where(s => s.SemesterId == selectedSemester.SemesterId)
                    .Sum(h => h.AmountHours) + selectedSemester.MinimumServiceHours > 0);
            var adjustedMemberCount = roster.Count();
            var events = await GetEventsBySemesterIdAsync(sid);
            var eventCount = events.Count();
            var now = DateTime.UtcNow;
            var monthYearTuples = MonthsBetween(priorSemester.DateEnd, selectedSemester.DateEnd);
            var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

            var includesUnapprovedHours = false;
            var months = monthYearTuples.Select(x => $"{dateTimeFormat.GetMonthName(x.Item1)} {x.Item2}");
            var moreThanZeroHoursSeries = new List<int>();
            var fiveOrMoreHoursSeries = new List<int>();
            var tenOrMoreHoursSeries = new List<int>();
            var fifteenOrMoreHoursSeries = new List<int>();
            var membersToHours = new Dictionary<int, double>();

            foreach (var monthYear in monthYearTuples)
            {
                var firstDayOfMonth = new DateTime(monthYear.Item2, monthYear.Item1, 1, 0, 0, 0, DateTimeKind.Utc);
                if (now < firstDayOfMonth) continue;

                var monthEvents = events
                    .Where(x =>
                        x.DateTimeOccurred.Month == monthYear.Item1 &&
                        x.DateTimeOccurred.Year == monthYear.Item2);
                // Add the month's submissions to lookup
                foreach (var e in monthEvents)
                {
                    if (!e.IsApproved)
                        includesUnapprovedHours = true;
                    foreach (var h in e.ServiceHours)
                    {
                        if (!membersToHours.ContainsKey(h.UserId))
                            membersToHours.Add(h.UserId, 0.0);

                        membersToHours[h.UserId] += h.DurationHours;
                    }
                }

                // Check lookup results to populate lists
                moreThanZeroHoursSeries.Add(membersToHours.Count);
                fiveOrMoreHoursSeries.Add(membersToHours.Count(x => x.Value >= 5));
                tenOrMoreHoursSeries.Add(membersToHours.Count(x => x.Value >= 10));
                fifteenOrMoreHoursSeries.Add(membersToHours.Count(x => x.Value >= 15));
            }


            model = new ServiceHourStats(
                unadjustedMemberCount,
                adjustedMemberCount,
                includesUnapprovedHours,
                months,
                moreThanZeroHoursSeries,
                fiveOrMoreHoursSeries,
                tenOrMoreHoursSeries,
                fifteenOrMoreHoursSeries);

            // Add model to cache
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(6)
            };
            cache.Set(sid.ToString(), model, policy);

            return model;
        }

        private static IEnumerable<Tuple<int, int>> MonthsBetween(DateTime startDate, DateTime endDate)
        {
            DateTime iterator;
            DateTime limit;

            if (endDate > startDate)
            {
                iterator = new DateTime(startDate.Year, startDate.Month, 1);
                limit = endDate;
            }
            else
            {
                iterator = new DateTime(endDate.Year, endDate.Month, 1);
                limit = startDate;
            }
            while (iterator <= limit)
            {
                yield return Tuple.Create(iterator.Month, iterator.Year);
                iterator = iterator.AddMonths(1);
            }
        }
    }
}
