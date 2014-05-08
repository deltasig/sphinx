namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using System;
    using System.Linq;
    using Interfaces;
    using Models;

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