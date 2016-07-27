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

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
