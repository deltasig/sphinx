using System.ComponentModel.DataAnnotations;

namespace Dsp.Web.Areas.Members.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
