namespace Dsp.Services
{
    using Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Repositories.Interfaces;
    using Dsp.Services.Exceptions;
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MealService : BaseService, IMealService
    {
        private readonly IRepository _repository;

        public MealService() : this(new Repository<SphinxDbContext>(new SphinxDbContext()))
        {

        }

        public MealService(SphinxDbContext db) : this(new Repository<SphinxDbContext>(db))
        {

        }

        public MealService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MealPeriod>> GetAllPeriodsAsync()
        {
            return await _repository.GetAllAsync<MealPeriod>();
        }

        public async Task<IEnumerable<MealItem>> GetAllItemsAsync()
        {
            return await _repository.GetAllAsync<MealItem>();
        }

        public async Task<IEnumerable<MealItemVote>> GetAllVotesByUserIdAsync(int userId)
        {
            return await _repository.GetAsync<MealItemVote>(filter: e => e.UserId == userId);
        }

        public async Task<IEnumerable<MealItemToPeriod>> GetMealItemToPeriodsAsync(DateTime start, DateTime end)
        {
            return await _repository.GetAsync<MealItemToPeriod>(e => start <= e.Date && e.Date < end);
        }

        public async Task<IEnumerable<MealPlate>> GetMealPlatesAsync(DateTime start, DateTime end)
        {
            return await _repository.GetAsync<MealPlate>(e => start <= e.PlateDateTime && e.PlateDateTime < end);
        }

        public async Task<MealPlate> GetPlateByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<MealPlate>(id);
        }

        public async Task<MealItem> GetItemByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<MealItem>(id);
        }

        public async Task<MealPeriod> GetPeriodByIdAsync(int id)
        {
            return await _repository.GetByIdAsync<MealPeriod>(id);
        }

        public async Task CreateItem(MealItem entity)
        {
            entity.Name = base.ToTitleCaseString(entity.Name);
            entity.Name = entity.Name.Replace("And", "&");

            var exists = await _repository.GetExistsAsync<MealItem>(m => m.Name == entity.Name);
            if (exists)
            {
                throw new MealItemAlreadyExistsException("A meal item with that name already exists.");
            }

            _repository.Create(entity);
            await _repository.SaveAsync();
        }

        public async Task CreatePeriod(MealPeriod entity)
        {
            _repository.Create(entity);
            await _repository.SaveAsync();
        }

        public async Task CreatePlate(MealPlate entity)
        {
            var existingPlates = await _repository
                .GetAsync<MealPlate>(
                    filter: v => v.UserId == entity.UserId && v.PlateDateTime == entity.PlateDateTime);

            if (entity.Type == "Late" && existingPlates.Any(p => p.Type == "Late"))
            {
                entity.Type = "+1";
            }

            _repository.Create(entity);
            await _repository.SaveAsync();
        }

        public async Task CreateItemToPeriod(MealItemToPeriod entity)
        {
            var exists = await _repository
                .GetExistsAsync<MealItemToPeriod>(
                    e => e.MealItemId == entity.MealItemId && e.MealPeriodId == entity.MealPeriodId && e.Date == entity.Date);
            if (exists)
            {
                throw new MealItemAlreadyAssignedException("A meal item has already been added to that period.");
            }

            _repository.Create(entity);
            await _repository.SaveAsync();
        }

        public async Task UpdateItem(MealItem entity)
        {
            entity.Name = base.ToTitleCaseString(entity.Name);
            entity.Name = entity.Name.Replace("And", "&");

            _repository.Update(entity);
            await _repository.SaveAsync();
        }

        public async Task UpdatePeriod(MealPeriod period)
        {
            _repository.Update(period);
            await _repository.SaveAsync();
        }

        public async Task DeleteItem(int id)
        {
            _repository.Delete<MealItem>(id);
            await _repository.SaveAsync();
        }

        public async Task DeletePeriod(int id)
        {
            _repository.Delete<MealPeriod>(id);
            await _repository.SaveAsync();
        }

        public async Task DeletePlate(int id)
        {
            _repository.Delete<MealPlate>(id);
            await _repository.SaveAsync();
        }

        public async Task DeleteItemFromPeriod(int id)
        {
            _repository.Delete<MealItemToPeriod>(id);
            await _repository.SaveAsync();
        }

        public async Task ProcessVote(MealItemVote entity)
        {
            var existingVote = await _repository
                .GetOneAsync<MealItemVote>(
                    e => e.UserId == entity.UserId && e.MealItemId == entity.MealItemId);

            if (existingVote == null)
            {
                _repository.Create(entity);
            }
            else if (existingVote.IsUpvote == entity.IsUpvote)
            {
                _repository.Delete(existingVote);
            }
            else
            {
                existingVote.IsUpvote = entity.IsUpvote;
                _repository.Update(existingVote);
            }

            await _repository.SaveAsync();
        }
    }
}
