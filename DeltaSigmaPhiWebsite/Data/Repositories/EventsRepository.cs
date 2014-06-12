namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;
    using Models.Entities;

    public class EventsRepository : GenericRepository<Event>, IEventsRepository
    {
        public EventsRepository(DspContext context) : base(context)
        {
        }
    }
}