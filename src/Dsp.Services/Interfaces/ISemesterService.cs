namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISemesterService
    {
        IList<string> Alphabet { get; }

        Task<IEnumerable<Semester>> GetAllSemestersAsync();
        Task<IEnumerable<Semester>> GetCurrentAndNextSemesterAsync();
        Task<Semester> GetCurrentSemesterAsync();
        Task<Semester> GetSemesterByIdAsync(int id);
        Task<Semester> GetFutureMostSemesterAsync();
        Semester GetEstimatedNextSemester(Semester currentSemester);
        PledgeClass GetEstimatedNextPledgeClass(Semester currentSemester);
        string GetNextPledgeClassName(string currentPledgeClassName);

        Task CreateSemesterAsync(Semester semester);
        Task CreatePledgeClassAsync(PledgeClass pledgeClass);

        Task UpdateSemesterAsync(Semester semester);
        Task UpdatePledgeClassAsync(PledgeClass pledgeClass);

        Task DeleteSemesterAsync(int id);
        Task DeletePledgeClassAsync(int id);
    }
}