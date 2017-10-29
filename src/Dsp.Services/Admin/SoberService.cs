namespace Dsp.Services
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Services.Admin;
    using Dsp.Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    public class SoberService : BaseService, ISoberService
    {
        private readonly ISemesterService _semesterService;

        public SoberService(SphinxDbContext db) : base(db)
        {
            _semesterService = new SemesterService(db);
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
            var futureSignups = await _db.SoberSignups
                .Where(s => s.DateOfShift >= startOfTodayUtc &&
                            s.DateOfShift <= thisSemester.DateEnd)
                .OrderBy(s => s.DateOfShift)
                .ThenBy(s => s.SoberTypeId)
                .ToListAsync();
            var data = new List<SoberSignup>();
            for (int i = 0; i < futureSignups.Count; i++)
            {
                data.Add(futureSignups[i]);
                if (i == futureSignups.Count - 1 ||
                    (futureSignups[i].DateOfShift != futureSignups[i + 1].DateOfShift &&
                    futureSignups[i].DateOfShift.AddDays(1) != futureSignups[i + 1].DateOfShift))
                    break;
            }
            return data;
        }
    }
}