namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using Models;

    public class EventsRepository : GenericRepository<Event>, IEventsRepository
    {
        public EventsRepository(DspContext context) : base(context)
        {
        }
    }
}