namespace Dsp.Web.Areas.Members.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class InitiatePledgesModel
    {
        public int[] SelectedMemberIds { get; set; }
        public IEnumerable<SelectListItem> Pledges { get; set; }
    }
}