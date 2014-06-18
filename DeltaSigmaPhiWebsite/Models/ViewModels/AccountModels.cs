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

    public class AppointmentsModel
    {
        public AppointModel AppointModel { get; set; }
        public AppointModel UnappointModel { get; set; }
        public CreatePositionModel CreateModel { get; set; }
        public DeletePositionModel DeleteModel { get; set; }
        public IEnumerable<Leader> RecentAppointments { get; set; }
    }

    public class AppointModel
    {
        [Display(Name = "Member")]
        public int SelectedUserId { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
        [Display(Name = "Position")]
        public string SelectedPositionName { get; set; }
        public IEnumerable<SelectListItem> Positions { get; set; }
        [Display(Name = "Semester")]
        public int SelectedSemesterId { get; set; }
        public IEnumerable<SelectListItem> AvailableSemesters { get; set; } 
    }

    public class CreatePositionModel
    {
        [Required]
        [Display(Name = "Position")]
        [DataType(DataType.Text)]
        public string PositionName { get; set; }
    }

    public class DeletePositionModel
    {
        [Required]
        [Display(Name = "Position")]
        public string SelectedPositionName { get; set; }
        public IEnumerable<SelectListItem> Positions { get; set; }
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

    public class EditMemberInfoModel
    {
        public Member Member { get; set; }

        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> PledgeClasses { get; set; }
        public IEnumerable<SelectListItem> Semesters { get; set; }
        public IEnumerable<SelectListItem> Members { get; set; }
    }

    public class RosterModel
    {
        public IEnumerable<Member> Members { get; set; } 
        public RosterSearchModel SearchModel { get; set; }
    }

    public class RosterSearchModel
    {
        public string SearchTerm { get; set; }

        public int SelectedStatusId { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public int SelectedPledgeClassId { get; set; }
        public IEnumerable<SelectListItem> PledgeClasses { get; set; }
        public int SelectedGraduationSemesterId { get; set; }
        public IEnumerable<SelectListItem> Semesters { get; set; }

        public string LivingType { get; set; }

        public bool CustomSearchRequested()
        {
            return LivingType == "InHouse" ||
                LivingType == "OutOfHouse" ||
                SelectedStatusId != -1 || 
                SelectedPledgeClassId != -1 ||
                SelectedGraduationSemesterId != -1;
        }
    }
}
