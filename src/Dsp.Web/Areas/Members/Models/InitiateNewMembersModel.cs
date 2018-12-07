namespace Dsp.Web.Areas.Members.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class InitiateNewMembersModel
    {
        public int[] SelectedMemberIds { get; set; }
        public IEnumerable<SelectListItem> NewMembers { get; set; }
    }
}