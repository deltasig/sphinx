namespace Dsp.Services.Interfaces
{
    using Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBroQuestService
    {
        Task<IEnumerable<QuestChallenge>> GetChallengesForMemberAsync(int mid, int sid);
        Task<QuestChallenge> GetChallengeAsync(int id);
        Task<QuestChallenge> GetChallengeAsync(int mid, int sid);
        Task<QuestCompletion> GetCompletionAsync(int id);
        Task<QuestCompletion> GetCompletionAsync(int cid, int nmid);
        Task AddChallengeAsync(int mid, int sid, DateTime start, DateTime end);
        Task AddChallengeAsync(QuestChallenge challenge);
        Task DeleteChallengeAsync(int id);
        Task DeleteChallengeAsync(QuestChallenge challenge);
        Task AcceptChallengeAsync(int cid, int nmid, bool verified);
        Task AcceptChallengeAsync(QuestCompletion completion);
        Task UnacceptChallengeAsync(int id);
        Task UnacceptChallengeAsync(QuestCompletion completion);
    }
}