namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class MembersRepository : GenericRepository<Member>, IMembersRepository
    {
        public MembersRepository(DspContext context) : base(context)
        {
        }
    }
}