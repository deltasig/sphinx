namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class MemberStatusRepository : GenericRepository<MemberStatus>, IMemberStatusRepository
    {
        public MemberStatusRepository(DspContext context) : base(context)
        {
        }
    }
}