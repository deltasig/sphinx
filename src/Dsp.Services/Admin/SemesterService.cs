namespace Dsp.Services.Admin
{
    using Data;
    using Data.Entities;
    using Interfaces;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    public class SemesterService : BaseService, ISemesterService
    {
        public SemesterService(SphinxDbContext db) : base(db)
        {
        }

        public async Task<Semester> GetCurrentSemesterAsync()
        {
            return (await _db.Semesters
                    .Where(s => s.DateEnd >= DateTime.UtcNow)
                    .OrderBy(s => s.DateStart)
                    .ToListAsync())
                    .First();
        }
    }
}
