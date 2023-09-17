namespace Dsp.WebCore.Areas.Members.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ExternalLoginConfirmationViewModel
    {
        [Required, Display(Name = "Email")]
        public string Email { get; set; }
    }
}