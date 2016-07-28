namespace Dsp.Web.Areas.Nme.Models
{
    using Data.Entities;

    public class BroQuestChallengeModel
    {
        public Semester Semester { get; set; }
        public Member Member { get; set; }

        public BroQuestChallengeModel(Semester semester)
        {
            Semester = semester;
        }
        public BroQuestChallengeModel(Member member)
        {
            Member = member;
        }
        public BroQuestChallengeModel(Semester semester, Member member)
        {
            Semester = semester;
            Member = member;
        }
    }
}
