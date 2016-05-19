namespace Dsp.Web.Areas.Sphinx.Models
{
    using Entities;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

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