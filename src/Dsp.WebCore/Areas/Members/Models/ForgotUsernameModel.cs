namespace Dsp.WebCore.Areas.Members.Models;

using System.ComponentModel.DataAnnotations;

public class ForgotUsernameModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }
}
