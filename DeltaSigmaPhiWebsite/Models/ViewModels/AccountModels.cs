namespace DeltaSigmaPhiWebsite.Models.ViewModels
{
    using Entities;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember Me?")]
        public bool RememberMe { get; set; }
    }

    public class RegistrationModel
    {
        public RegisterModel RegisterModel { get; set; }
        public UnregisterModel UnregisterModel { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "S&T Email Address")]
        [RegularExpression(@"^[a-zA-Z0-9]([a-zA-Z0-9-_.])*@((m|M)(a|A)(i|I)(l|L).)?(m|M)(s|S)(t|T).(e|E)(d|D)(u|U)", ErrorMessage = "You must have an @mail.mst.edu or @mst.edu Email to register.")]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nickname")]
        public string Nickname { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Status")]
        public string StatusId { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UnregisterModel
    {
        [Display(Name = "Users")]
        public int SelectedUserId { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
    }

    public class RoleManagerModel
    {
        public AssignUserToRoleModel AssignModel { get; set; }
        public UnassignUserFromRoleModel UnassignModel { get; set; }
        public CreateRoleModel CreateModel { get; set; }
        public DeleteRoleModel DeleteModel { get; set; }
    }

    public class AssignUserToRoleModel
    {
        [Display(Name = "Assign")]
        public int SelectedUserId { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }

        [Display(Name = "As")]
        public string SelectedRoleName { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; }
    }

    public class UnassignUserFromRoleModel
    {
        [Display(Name = "Unassign")]
        public int SelectedUserId { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }

        [Display(Name = "From")]
        public string SelectedRoleName { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; }
    }

    public class CreateRoleModel
    {
        [Required]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }

    public class DeleteRoleModel
    {
        [Display(Name = "Roles")]
        public string SelectedRoleName { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }
    }
    
    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
        public string ProviderUserName { get; set; }
    }

    public class AccountInformationModel
    {
        public Member MemberInfo { get; set; }
        public LocalPasswordModel ChangePasswordModel { get; set; }
        public string ProfilePicUrl { get; set; }
    }
}
