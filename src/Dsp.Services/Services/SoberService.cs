namespace Dsp.Services;

using Dsp.Data;
using Dsp.Data.Entities;
using Dsp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SoberService : BaseService, ISoberService
{
    private readonly DspDbContext _context;
    private readonly ISemesterService _semesterService;

    public SoberService(DspDbContext context, ISemesterService semesterService)
    {
        _context = context;
        _semesterService = semesterService;
    }

    public virtual async Task<IEnumerable<SoberSignup>> GetUpcomingSignupsAsync()
    {
        return await GetUpcomingSignupsAsync(DateTime.UtcNow);
    }

    public virtual async Task<IEnumerable<SoberSignup>> GetUpcomingSignupsAsync(DateTime date)
    {
        var dateCst = ConvertUtcToCst(date);
        if (dateCst.Hour < 6) // Don't show next day until after 6am
        {
            date = date.AddDays(-1);
        }

        var startOfTodayCst = ConvertUtcToCst(date).Date;
        var startOfTodayUtc = ConvertCstToUtc(startOfTodayCst);
        var thisSemester = await _semesterService.GetCurrentSemesterAsync();
        var futureSignups = await _context.SoberSignups
            .Where(s =>
                s.DateOfShift >= startOfTodayUtc &&
                s.DateOfShift <= thisSemester.DateEnd)
            .ToListAsync();
        var orderedSignups = futureSignups
            .OrderBy(s => s.DateOfShift)
            .ThenBy(s => s.SoberTypeId)
            .ToList();
        var signups = new List<SoberSignup>();
        for (int i = 0; i < orderedSignups.Count; i++)
        {
            signups.Add(orderedSignups[i]);
            if (i == orderedSignups.Count - 1 ||
                (orderedSignups[i].DateOfShift != orderedSignups[i + 1].DateOfShift &&
                orderedSignups[i].DateOfShift.AddDays(1) != orderedSignups[i + 1].DateOfShift))
                break;
        }
        return signups;
    }

    public async Task<IEnumerable<SoberSignup>> GetAllFutureSignupsAsync()
    {
        return await GetAllFutureSignupsAsync(DateTime.UtcNow);
    }

    public async Task<IEnumerable<SoberSignup>> GetAllFutureSignupsAsync(DateTime start)
    {
        var threeAmYesterday = ConvertCstToUtc(ConvertUtcToCst(start).Date).AddDays(-1).AddHours(3);

        var signups = await _context.SoberSignups
            .Where(s => s.DateOfShift >= threeAmYesterday)
            .OrderBy(s => s.DateOfShift)
            .Include(s => s.SoberType)
            .ToListAsync();

        return signups;
    }

    public async Task<IEnumerable<SoberSignup>> GetFutureVacantSignups()
    {
        var startOfTodayUtc = ConvertCstToUtc(ConvertUtcToCst(DateTime.UtcNow).Date);
        var vacantSignups = await _context.SoberSignups
            .Where(s => s.DateOfShift >= startOfTodayUtc && s.UserId == null)
            .OrderBy(s => s.DateOfShift)
            .Include(x => x.SoberType)
            .ToListAsync();
        return vacantSignups;
    }

    public async Task<IEnumerable<SoberType>> GetTypesAsync()
    {
        return await _context.SoberTypes.ToListAsync();
    }

    public async Task CreateSignupAsync(SoberSignup signup)
    {
        _context.Add(signup);
        await _context.SaveChangesAsync();
    }

    public async Task CreateSignupsAsync(IEnumerable<SoberSignup> signups)
    {
        foreach (var s in signups)
        {
            _context.Add(s);
        }
        await _context.SaveChangesAsync();
    }

    public async Task CreateTypeAsync(SoberType type)
    {
        _context.Add(type);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSignupAsync(SoberSignup signup)
    {
        _context.Update(signup);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTypeAsync(SoberType type)
    {
        _context.Update(type);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSignupAsync(int id)
    {
        var entity = new SoberSignup { SignupId = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTypeAsync(int id)
    {
        var entity = new SoberType { SoberTypeId = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }
}