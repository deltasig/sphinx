namespace Dsp.WebCore.Areas.Sobers.Models
{
    using Dsp.Data.Entities;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class EditSoberSignupModel
    {
        public SoberSignup SoberSignup { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Member")]
        public int SelectedMember { get; set; }
        public IEnumerable<SelectListItem> Members { get; set; }
    }
}