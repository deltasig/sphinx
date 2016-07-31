namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBroQuestService
    {
        Task<IEnumerable<QuestChallenge>> GetChallengesForMemberAsync(int mid, int sid);
        Task AddChallengeAsync(int mid, int sid, DateTime? start, DateTime? end);
        Task AddChallengeAsync(QuestChallenge challenge);
        Task DeleteChallengeAsync(int id);
        Task DeleteChallengeAsync(QuestChallenge challenge);
    }
}