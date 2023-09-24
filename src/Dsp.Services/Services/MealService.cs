namespace Dsp.Services;

using Data;
using Dsp.Data.Entities;
using Dsp.Services.Exceptions;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MealService : BaseService, IMealService
{
    private readonly DspDbContext _context;

    public MealService(DspDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MealPeriod>> GetAllPeriodsAsync()
    {
        return await _context.MealPeriods.ToListAsync();
    }

    public async Task<IEnumerable<MealItem>> GetAllItemsAsync()
    {
        return await _context.MealItems.ToListAsync();
    }

    public async Task<IEnumerable<MealItemVote>> GetAllVotesByUserIdAsync(int userId)
    {
        return await _context.MealItemVotes
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<MealItemToPeriod>> GetMealItemToPeriodsAsync(DateTime start, DateTime end)
    {
        return await _context.MealItemsToPeriods
            .Where(e => start <= e.Date && e.Date < end)
            .ToListAsync();
    }

    public async Task<IEnumerable<MealPlate>> GetMealPlatesAsync(DateTime start, DateTime end)
    {
        return await _context.MealPlates
            .Where(e => start <= e.PlateDateTime && e.PlateDateTime < end)
            .ToListAsync();
    }

    public async Task<MealPlate> GetPlateByIdAsync(int id)
    {
        return await _context.FindAsync<MealPlate>(id);
    }

    public async Task<MealItem> GetItemByIdAsync(int id)
    {
        return await _context.FindAsync<MealItem>(id);
    }

    public async Task<MealPeriod> GetPeriodByIdAsync(int id)
    {
        return await _context.FindAsync<MealPeriod>(id);
    }

    public async Task CreateItem(MealItem entity)
    {
        entity.Name = base.ToTitleCaseString(entity.Name);
        entity.Name = entity.Name.Replace("And", "&");

        var exists = await _context.MealItems.AnyAsync(m => m.Name == entity.Name);
        if (exists)
        {
            throw new MealItemAlreadyExistsException("A meal item with that name already exists.");
        }

        _context.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task CreatePeriod(MealPeriod entity)
    {
        _context.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task CreatePlate(MealPlate entity)
    {
        var existingPlates = await _context.MealPlates
            .Where(v => v.UserId == entity.UserId && v.PlateDateTime == entity.PlateDateTime)
            .ToListAsync();

        if (entity.Type == "Late" && existingPlates.Any(p => p.Type == "Late"))
        {
            entity.Type = "+1";
        }

        _context.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task CreateItemToPeriod(MealItemToPeriod entity)
    {
        var exists = await _context.MealItemsToPeriods.AnyAsync(
            e => e.MealItemId == entity.MealItemId && 
            e.MealPeriodId == entity.MealPeriodId && 
            e.Date == entity.Date
        );
        if (exists)
        {
            throw new MealItemAlreadyAssignedException("A meal item has already been added to that period.");
        }

        _context.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItem(MealItem entity)
    {
        entity.Name = base.ToTitleCaseString(entity.Name);
        entity.Name = entity.Name.Replace("And", "&");

        _context.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePeriod(MealPeriod period)
    {
        _context.Update(period);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteItem(int id)
    {
        var entity = new MealItem { Id = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePeriod(int id)
    {
        var entity = new MealPeriod { Id = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePlate(int id)
    {
        var entity = new MealPlate { Id = id };
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteItemFromPeriod(int id)
    {
        var entity = new MealItemToPeriod { Id = id };
        await _context.SaveChangesAsync();
    }

    public async Task ProcessVote(MealItemVote entity)
    {
        var existingVote = await _context.MealItemVotes
            .Where(e => e.UserId == entity.UserId && e.MealItemId == entity.MealItemId)
            .FirstOrDefaultAsync();

        if (existingVote == null)
        {
            _context.Add(entity);
        }
        else if (existingVote.IsUpvote == entity.IsUpvote)
        {
            _context.Remove(existingVote);
        }
        else
        {
            existingVote.IsUpvote = entity.IsUpvote;
            _context.Update(existingVote);
        }

        await _context.SaveChangesAsync();
    }
}
