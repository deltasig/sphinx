namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;
    using System;
    using System.Linq;

    public class LaundrySignupRepository : GenericRepository<LaundrySignup>, ILaundrySignupRepository
    {
        public LaundrySignupRepository(DspContext context) : base(context)
        {

        }

        public void DeleteByShift(DateTime dateTime)
        {
            var entityToDelete = _context.Set<LaundrySignup>().Single(s => s.DateTimeShift == dateTime);
            Delete(entityToDelete);
        }
    }
}