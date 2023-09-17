using System.ComponentModel.DataAnnotations;

namespace Dsp.WebCore.Areas.Members.Models
{
    public class ResendConfirmationEmailModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
