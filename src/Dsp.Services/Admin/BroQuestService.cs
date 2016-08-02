namespace Dsp.Services.Admin
{
    using Data;
    using Data.Entities;
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    public class BroQuestService : BaseService, IBroQuestService
    {
        public BroQuestService(SphinxDbContext db) : base(db)
        {
        }

        public async Task<IEnumerable<QuestChallenge>> GetChallengesForMemberAsync(int mid, int sid)
        {
            return await _db.QuestChallenges
                .Where(c => c.MemberId == mid && c.SemesterId == sid)
                .ToListAsync();
        }

        public async Task<QuestChallenge> GetChallengeAsync(int id)
        {
            return await _db.QuestChallenges.FindAsync(id);
        }

        public async Task<QuestChallenge> GetChallengeAsync(int mid, int sid)
        {
            return await _db.QuestChallenges
                .SingleOrDefaultAsync(c => c.MemberId == mid && c.SemesterId == sid);
        }
        
        public async Task<QuestCompletion> GetCompletionAsync(int id)
        {
            return await _db.QuestCompletions.FindAsync(id);
        }

        public async Task<QuestCompletion> GetCompletionAsync(int cid, int nmid)
        {
            return await _db.QuestCompletions
                .SingleOrDefaultAsync(c => c.ChallengeId == cid && c.NewMemberId == nmid);
        }

        public async Task AddChallengeAsync(int mid, int sid, DateTime start, DateTime end)
        {
            var challenge = new QuestChallenge
            {
                MemberId = mid,
                SemesterId = sid,
                BeginsOn = start,
                EndsOn = end
            };
            await AddChallengeAsync(challenge);
        }

        public async Task AddChallengeAsync(QuestChallenge challenge)
        {
            _db.QuestChallenges.Add(challenge);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteChallengeAsync(int id)
        {
            var challenge = await _db.QuestChallenges.FindAsync(id);
            await DeleteChallengeAsync(challenge);
        }

        public async Task DeleteChallengeAsync(QuestChallenge challenge)
        {
            _db.Entry(challenge).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
        }

        public async Task AcceptChallengeAsync(int cid, int nmid, bool verified)
        {
            var completion = new QuestCompletion
            {
                ChallengeId = cid,
                NewMemberId = nmid,
                IsVerified = verified
            };
            await AcceptChallengeAsync(completion);
        }

        public async Task AcceptChallengeAsync(QuestCompletion completion)
        {
            _db.QuestCompletions.Add(completion);
            await _db.SaveChangesAsync();
        }
        
        public async Task UnacceptChallengeAsync(int id)
        {
            var completion = await GetCompletionAsync(id);            
            await UnacceptChallengeAsync(completion);
        }

        public async Task UnacceptChallengeAsync(QuestCompletion completion)
        {
            _db.Entry(completion).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
        }
    }
}
