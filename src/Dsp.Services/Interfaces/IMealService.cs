namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMealService : IService
    {
        Task<IEnumerable<MealPeriod>> GetAllPeriodsAsync();
        Task<IEnumerable<MealItem>> GetAllItemsAsync();
        Task<IEnumerable<MealItemVote>> GetAllVotesByUserIdAsync(int userId);
        Task<IEnumerable<MealItemToPeriod>> GetMealItemToPeriodsAsync(DateTime start, DateTime end);
        Task<IEnumerable<MealPlate>> GetMealPlatesAsync(DateTime start, DateTime end);
        Task<MealItem> GetItemByIdAsync(int id);
        Task<MealPeriod> GetPeriodByIdAsync(int id);
        Task<MealPlate> GetPlateByIdAsync(int id);

        Task CreateItem(MealItem entity);
        Task CreatePeriod(MealPeriod entity);
        Task CreatePlate(MealPlate entity);
        Task CreateItemToPeriod(MealItemToPeriod entity);

        Task UpdateItem(MealItem entity);
        Task UpdatePeriod(MealPeriod entity);

        Task DeleteItem(int id);
        Task DeletePeriod(int id);
        Task DeletePlate(int id);
        Task DeleteItemFromPeriod(int id);

        Task ProcessVote(MealItemVote entity);
    }
}