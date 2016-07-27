namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System.Threading.Tasks;

    public interface ISemesterService
    {
        Task<Semester> GetCurrentSemesterAsync();
        Task<Semester> GetSemesterByIdAsync(int id);
    }
}