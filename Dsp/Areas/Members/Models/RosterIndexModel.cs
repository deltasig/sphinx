namespace Dsp.Areas.Members.Models
{
    using Entities;
    using System.Collections.Generic;

    public class RosterIndexModel
    {
        public IEnumerable<Member> Members { get; set; }
        public RosterIndexSearchModel SearchModel { get; set; }
    }
}