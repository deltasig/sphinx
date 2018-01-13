namespace Dsp.Services
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Dsp.Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SoberService : BaseService, ISoberService
    {
        private readonly IRepository _repository;
        private readonly ISemesterService _semesterService;

        public SoberService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public SoberService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public SoberService(IRepository repository)
        {
            _repository = repository;
            _semesterService = new SemesterService(repository);
        }

        public virtual async Task<IEnumerable<SoberSignup>> GetUpcomingSignupsAsync()
        {
            return await GetUpcomingSignupsAsync(DateTime.UtcNow);
        }

        public virtual async Task<IEnumerable<SoberSignup>> GetUpcomingSignupsAsync(DateTime date)
        {
            var dateCst = ConvertUtcToCst(date);
            if (dateCst.Hour < 6) // Don't show next day until after 6am
            {
                date = date.AddDays(-1);
            }

            var startOfTodayCst = ConvertUtcToCst(date).Date;
            var startOfTodayUtc = ConvertCstToUtc(startOfTodayCst);
            var thisSemester = await _semesterService.GetCurrentSemesterAsync();
            var futureSignups = await _repository
                .GetAsync<SoberSignup>(s =>
                    s.DateOfShift >= startOfTodayUtc &&
                    s.DateOfShift <= thisSemester.DateEnd);
            var orderedSignups = futureSignups
                .OrderBy(s => s.DateOfShift)
                .ThenBy(s => s.SoberTypeId)
                .ToList();
            var signups = new List<SoberSignup>();
            for (int i = 0; i < orderedSignups.Count; i++)
            {
                signups.Add(orderedSignups[i]);
                if (i == orderedSignups.Count - 1 ||
                    (orderedSignups[i].DateOfShift != orderedSignups[i + 1].DateOfShift &&
                    orderedSignups[i].DateOfShift.AddDays(1) != orderedSignups[i + 1].DateOfShift))
                    break;
            }
            return signups;
        }

        public async Task<IEnumerable<SoberSignup>> GetAllFutureSignupsAsync()
        {
            return await GetAllFutureSignupsAsync(DateTime.UtcNow);
        }

        public async Task<IEnumerable<SoberSignup>> GetAllFutureSignupsAsync(DateTime start)
        {
            var threeAmYesterday = ConvertCstToUtc(ConvertUtcToCst(start).Date).AddDays(-1).AddHours(3);

            var signups = await _repository
                .GetAsync<SoberSignup>(
                    s => s.DateOfShift >= threeAmYesterday,
                    o => o.OrderBy(s => s.DateOfShift));

            return signups;
        }

        public async Task<IEnumerable<SoberSignup>> GetFutureVacantSignups()
        {
            var startOfTodayUtc = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date);
            var vacantSignups = await _repository
                .GetAsync<SoberSignup>(
                    s => s.DateOfShift >= startOfTodayUtc && s.UserId == null,
                    o => o.OrderBy(s => s.DateOfShift),
                    includeProperties: "SoberType");
            return vacantSignups;
        }

        public async Task<IEnumerable<SoberType>> GetTypesAsync()
        {
            return await _repository.GetAllAsync<SoberType>();
        }

        public async Task AddSignupAsync(SoberSignup signup)
        {
            _repository.Create(signup);
            await _repository.SaveAsync();
        }

        public async Task AddSignupsAsync(IEnumerable<SoberSignup> signups)
        {
            foreach (var s in signups)
            {
                _repository.Create(s);
            }
            await _repository.SaveAsync();
        }

        public async Task AddTypeAsync(SoberType type)
        {
            _repository.Create(type);
            await _repository.SaveAsync();
        }

        public async Task UpdateSignupAsync(SoberSignup signup)
        {
            _repository.Update(signup);
            await _repository.SaveAsync();
        }

        public async Task UpdateTypeAsync(SoberType type)
        {
            _repository.Update(type);
            await _repository.SaveAsync();
        }

        public async Task DeleteSignupAsync(int id)
        {
            _repository.Delete<SoberSignup>(id);
            await _repository.SaveAsync();
        }

        public async Task DeleteTypeAsync(int id)
        {
            _repository.Delete<SoberType>(id);
            await _repository.SaveAsync();
        }
    }
}