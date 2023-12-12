namespace Dsp.WebCore.Areas.Members.Models;

using System.ComponentModel.DataAnnotations;

public class ResendConfirmationEmailModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }
}
