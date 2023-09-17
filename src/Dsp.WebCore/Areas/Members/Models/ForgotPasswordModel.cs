using System.ComponentModel.DataAnnotations;

namespace Dsp.WebCore.Areas.Members.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
