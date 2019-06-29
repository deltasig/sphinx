namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class EditMemberInfoModel
    {
        public Member Member { get; set; }

        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> PledgeClasses { get; set; }
        public IEnumerable<SelectListItem> Semesters { get; set; }
        public IEnumerable<SelectListItem> Members { get; set; }
    }
}
