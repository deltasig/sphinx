namespace Dsp.Web.Areas.Members.Models
{
    public class MemberStatusFilter
    {
        public bool Pledges { get; set; }
        public bool Neophytes { get; set; }
        public bool Actives { get; set; }
        public bool Alumni { get; set; }
        public bool Affiliates { get; set; }
        public bool Released { get; set; }

        internal bool IsBlank()
        {
            return !Pledges && !Neophytes && !Actives && !Alumni && !Affiliates && !Released;
        }
    }
}