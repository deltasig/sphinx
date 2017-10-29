using Dsp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsp.Services
{
    public abstract class BaseService : IService
    {
        internal readonly SphinxDbContext _db;

        public BaseService(SphinxDbContext db)
        {
            _db = db;
        }

        public virtual DateTime ConvertUtcToCst(DateTime utc)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, cstZone);
        }

        public virtual DateTime ConvertCstToUtc(DateTime cst)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(cst, cstZone);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
