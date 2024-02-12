namespace Dsp.Services;

using Data;
using Data.Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SemesterService : BaseService, ISemesterService
{
    private readonly DspDbContext _context;

    public IList<string> GreekAlphabet { get; } = new List<string>
    {
        "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta",
        "Theta", "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron",
        "Pi", "Rho", "Sigma", "Tau", "Upsilon", "Phi", "Chi", "Psi", "Omega"
    };

    public SemesterService(DspDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Semester>> GetAllSemestersAsync()
    {
        var semesters = await _context.Semesters
            .Include(x => x.PledgeClasses)
            .OrderBy(x => x.DateStart)
            .OrderByDescending(x => x.DateStart)
            .ToListAsync();
        return semesters;
    }

    public async Task<IEnumerable<PledgeClass>> GetAllPledgeClassesAsync()
    {
        var pledgeClasses = await _context.PledgeClasses
            .Include(x => x.Semester)
            .OrderByDescending(x => x.Semester.DateEnd)
            .ToListAsync();
        return pledgeClasses;
    }

    public async Task<IEnumerable<Semester>> GetCurrentAndNextSemesterAsync(DateTime? now = null)
    {
        if (now == null)
            now = DateTime.UtcNow;

        var thisAndNextSemester = await _context.Semesters
            .Where(s => s.DateEnd >= now)
            .OrderByDescending(x => x.DateStart)
            .Take(2)
            .ToListAsync();
        return thisAndNextSemester;
    }

    public async Task<Semester> GetCurrentSemesterAsync(DateTime? now = null)
    {
        if (now == null)
            now = DateTime.UtcNow;

        var semesters = await _context.Semesters
            .Where(s => s.DateEnd >= DateTime.UtcNow)
            .OrderBy(s => s.DateStart)
            .Take(1)
            .ToListAsync();
        var currentSemester = semesters.SingleOrDefault();

        return currentSemester;
    }

    public async Task<Semester> GetSemesterByIdAsync(int id)
    {
        return await _context.FindAsync<Semester>(id);
    }

    public async Task<Semester> GetSemesterByUtcDateTimeAsync(DateTime datetime)
    {
        return await _context.Semesters
            .Where(x => datetime <= x.DateEnd)
            .OrderBy(x => x.DateEnd)
            .FirstAsync();
    }

    public async Task<Semester> GetFutureMostSemesterAsync()
    {
        var allSemesters = await GetAllSemestersAsync();
        return allSemesters.LastOrDefault();
    }

    public async Task<IEnumerable<Semester>> GetPriorSemestersAsync(Semester currentSemester)
    {
        var priorSemesters = await _context.Semesters
            .Where(x => x.DateEnd < currentSemester.DateEnd)
            .OrderBy(x => x.DateEnd)
            .ToListAsync();
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
            var alphabetPosition = GreekAlphabet.IndexOf(p);
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
                if (index >= GreekAlphabet.Count)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }
            }
            var letter = GreekAlphabet[index];
            sb.Append(GreekAlphabet[index]);
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
        if (semester.TransitionDate < semester.DateEnd)
            throw new ArgumentException("Semester transition date cannot preceed the semester end date.");

        _context.Add(semester);
        await _context.SaveChangesAsync();
    }

    public async Task CreatePledgeClassAsync(PledgeClass pledgeClass)
    {
        _context.Add(pledgeClass);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSemesterAsync(Semester semester)
    {
        if (semester.TransitionDate < semester.DateEnd)
            throw new ArgumentException("Semester transition date cannot preceed the semester end date.");

        _context.Update(semester);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePledgeClassAsync(PledgeClass pledgeClass)
    {
        _context.Update(pledgeClass);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSemesterAsync(int id)
    {
        var entity = new Semester { Id = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePledgeClassAsync(int id)
    {
        var entity = new PledgeClass { PledgeClassId = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
