namespace Dsp.WebCore.Areas.Members.Models
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;

    public class GraduateActivesModel
    {
        public int[] SelectedMemberIds { get; set; }
        public IEnumerable<SelectListItem> Actives { get; set; }
    }
}