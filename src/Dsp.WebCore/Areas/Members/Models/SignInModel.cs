﻿namespace Dsp.WebCore.Areas.Members.Models;

using System.ComponentModel.DataAnnotations;

public class SignInModel
{
    [Required]
    [Display(Name = "User name")]
    public string UserName { get; set; }

    [Required]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}