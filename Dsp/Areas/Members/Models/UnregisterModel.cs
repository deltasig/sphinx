namespace Dsp.Areas.Members.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class UnregisterModel
    {
        [Display(Name = "Users")]
        public int SelectedUserId { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
    }
}