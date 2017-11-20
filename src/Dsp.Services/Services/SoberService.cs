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

        public virtual async Task<IEnumerable<SoberSignup>> GetUpcomingSoberSignupsAsync()
        {
            return await GetUpcomingSoberSignupsAsync(DateTime.UtcNow);
        }

        public virtual async Task<IEnumerable<SoberSignup>> GetUpcomingSoberSignupsAsync(DateTime date)
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
            var data = new List<SoberSignup>();
            for (int i = 0; i < orderedSignups.Count; i++)
            {
                data.Add(orderedSignups[i]);
                if (i == orderedSignups.Count - 1 ||
                    (orderedSignups[i].DateOfShift != orderedSignups[i + 1].DateOfShift &&
                    orderedSignups[i].DateOfShift.AddDays(1) != orderedSignups[i + 1].DateOfShift))
                    break;
            }
            return data;
        }
    }
}