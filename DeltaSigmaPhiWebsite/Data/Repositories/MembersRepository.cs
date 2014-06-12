namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class MembersRepository : GenericRepository<Member>, IMembersRepository
    {
        public MembersRepository(DspContext context) : base(context)
        {
        }
    }
}