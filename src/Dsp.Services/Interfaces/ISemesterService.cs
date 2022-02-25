namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISemesterService : IService
    {
        IList<string> Alphabet { get; }

        Task<IEnumerable<Semester>> GetAllSemestersAsync();
        Task<IEnumerable<Semester>> GetCurrentAndNextSemesterAsync(DateTime? now = null);
        Task<Semester> GetCurrentSemesterAsync(DateTime? now = null);
        Task<Semester> GetSemesterByIdAsync(int id);
        Task<Semester> GetSemesterByUtcDateTimeAsync(DateTime datetime);
        Task<Semester> GetFutureMostSemesterAsync();
        Task<IEnumerable<Semester>> GetPriorSemestersAsync(Semester currentSemester);
        Task<Semester> GetPriorSemesterAsync(Semester currentSemester);
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