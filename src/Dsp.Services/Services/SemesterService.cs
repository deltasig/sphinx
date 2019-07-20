namespace Dsp.Services
{
    using Data;
    using Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SemesterService : BaseService, ISemesterService
    {
        private readonly IRepository _repository;

        public IList<string> Alphabet { get; } = new List<string>
        {
            "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta",
            "Theta", "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron",
            "Pi", "Rho", "Sigma", "Tau", "Upsilon", "Phi", "Chi", "Psi", "Omega"
        };

        public SemesterService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public SemesterService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public SemesterService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Semester>> GetAllSemestersAsync()
        {
            var semesters = await _repository
                .GetAllAsync<Semester>(
                    orderBy: o => o.OrderBy(s => s.DateStart));
            return semesters;
        }

        public async Task<IEnumerable<Semester>> GetCurrentAndNextSemesterAsync()
        {
            var thisAndNextSemester = await _repository
                .GetAsync<Semester>(
                    filter: s => s.DateEnd >= DateTime.UtcNow,
                    orderBy: o => o.OrderBy(s => s.DateStart),
                    take: 2);
            return thisAndNextSemester;
        }

        public async Task<Semester> GetCurrentSemesterAsync()
        {
            var semesters = await _repository
                .GetAsync<Semester>(
                    s => s.DateEnd >= DateTime.UtcNow,
                    x => x.OrderBy(s => s.DateStart),
                    take: 1);
            return semesters.SingleOrDefault();
        }

        public async Task<Semester> GetSemesterByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<Semester>(id);
        }

        public async Task<Semester> GetSemesterByUtcDateTimeAsync(DateTime datetime)
        {
            return await _repository.GetFirstAsync<Semester>(
                filter: x => datetime <= x.DateEnd,
                orderBy: x => x.OrderBy(o => o.DateEnd)
            );
        }

        public async Task<Semester> GetFutureMostSemesterAsync()
        {
            var allSemesters = await GetAllSemestersAsync();
            return allSemesters.LastOrDefault();
        }

        public async Task<IEnumerable<Semester>> GetPriorSemestersAsync(Semester currentSemester)
        {
            var priorSemesters = await _repository.GetAsync<Semester>(
                filter: x => x.DateEnd < currentSemester.DateEnd,
                orderBy: x => x.OrderBy(o => o.DateEnd)
            );
            return priorSemesters;
        }

        public async Task<Semester> GetPriorSemesterAsync(Semester currentSemester)
        {
            var priorSemesters = await GetPriorSemestersAsync(currentSemester);
            var priorSemester = priorSemesters.LastOrDefault() ?? new Semester
            {
                // This is the where they picked the very first semester in the system.
                DateEnd = currentSemester.DateStart
            };
            return priorSemester;
        }

        public string GetNextPledgeClassName(string currentPledgeClassName)
        {
            var nameParts = currentPledgeClassName.Split(' ');
            var nameIndeces = new List<int>();
            foreach (var p in nameParts)
            {
                var alphabetPosition = Alphabet.IndexOf(p);
                if (alphabetPosition >= 0)
                {
                    nameIndeces.Add(alphabetPosition);
                }
            }
            var sb = new StringBuilder();
            for (var i = 0; i < nameIndeces.Count; i++)
            {
                var index = nameIndeces[i];
                if (i + 1 >= nameIndeces.Count)
                {
                    if (index >= Alphabet.Count)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }
                }
                var letter = Alphabet[index];
                sb.Append(Alphabet[index]);
                sb.Append(" ");
            }
            return sb.ToString().TrimEnd();
        }

        public Semester GetEstimatedNextSemester(Semester currentSemester)
        {
            var nextSemester = new Semester();
            if (currentSemester != null)
            {
                var offset = currentSemester.DateStart.Month < 5 ? 7 : 5;

                nextSemester.MinimumServiceEvents = currentSemester.MinimumServiceEvents;
                nextSemester.MinimumServiceHours = currentSemester.MinimumServiceHours;
                nextSemester.DateStart = currentSemester.DateStart.AddMonths(offset);
                nextSemester.DateEnd = currentSemester.DateEnd.AddMonths(offset);
                nextSemester.TransitionDate = currentSemester.TransitionDate.AddMonths(offset);
            }
            return nextSemester;
        }

        public PledgeClass GetEstimatedNextPledgeClass(Semester currentSemester)
        {
            var nextPledgeClass = new PledgeClass();
            if (currentSemester != null)
            {
                var currentPledgeClass = currentSemester.PledgeClasses.FirstOrDefault();
                var offset = currentSemester.DateStart.Month < 5 ? 7 : 5;

                if (currentPledgeClass.InitiationDate != null)
                {
                    nextPledgeClass.InitiationDate = ((DateTime)currentPledgeClass.InitiationDate).AddMonths(offset);
                }
                if (currentPledgeClass.PinningDate != null)
                {
                    nextPledgeClass.PinningDate = ((DateTime)currentPledgeClass.PinningDate).AddMonths(offset);
                }
                nextPledgeClass.PledgeClassName = GetNextPledgeClassName(currentPledgeClass.PledgeClassName);
            }
            return nextPledgeClass;
        }

        public async Task CreateSemesterAsync(Semester semester)
        {
            _repository.Create(semester);
            await _repository.SaveAsync();
        }

        public async Task CreatePledgeClassAsync(PledgeClass pledgeClass)
        {
            _repository.Create(pledgeClass);
            await _repository.SaveAsync();
        }

        public async Task UpdateSemesterAsync(Semester semester)
        {
            _repository.Update(semester);
            await _repository.SaveAsync();
        }

        public async Task UpdatePledgeClassAsync(PledgeClass pledgeClass)
        {
            _repository.Update(pledgeClass);
            await _repository.SaveAsync();
        }

        public async Task DeleteSemesterAsync(int id)
        {
            _repository.Delete<Semester>(id);
            await _repository.SaveAsync();
        }

        public async Task DeletePledgeClassAsync(int id)
        {
            _repository.Delete<PledgeClass>(id);
            await _repository.SaveAsync();
        }
    }
}
