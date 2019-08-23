using System.ComponentModel.DataAnnotations;

namespace Dsp.Web.Areas.Members.Models
{
    public class ForgotUsernameModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
