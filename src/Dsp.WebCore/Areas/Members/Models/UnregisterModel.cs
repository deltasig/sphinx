namespace Dsp.WebCore.Areas.Members.Models
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class UnregisterModel
    {
        [Display(Name = "Users")]
        public int SelectedUserId { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
    }
}