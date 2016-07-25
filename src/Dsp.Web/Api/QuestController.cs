using Dsp.Data;
using Dsp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Dsp.Web.Api
{
    [RoutePrefix("api/broquest")]
    public class BroQuestController : ApiController
    {
        private SphinxDbContext _db = new SphinxDbContext();
        
        [Route("period/{id:int}")]
        public async Task<IHttpActionResult> GetPeriod(int id)
        {
            var semester = await GetSemesterByIdAsync(id);
            if(semester == null)
            {
                return NotFound();
            }
            return Ok(semester);
        }

        private async Task<Semester> GetSemesterByIdAsync(int id)
        {
            return (await _db.Semesters
                    .Where(s => s.DateEnd >= DateTime.UtcNow)
                    .OrderBy(s => s.DateStart)
                    .ToListAsync())
                    .First();
        }
    }
}
