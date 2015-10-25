namespace Dsp.Areas.Members.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class GraduateActivesModel
    {
        public int[] SelectedMemberIds { get; set; }
        public IEnumerable<SelectListItem> Actives { get; set; }
    }
}