namespace DeltaSigmaPhiWebsite.Models.ViewModels
{
    using Entities;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    
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
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
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
        [DataType(DataType.Text)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Status")]
        public string StatusId { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
        
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Expected Graduation")]
        public string ExpectedGraduationId { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Pledge Class")]
        public string PledgeClassId { get; set; }
        public IEnumerable<SelectListItem> PledgeClassList { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(3, ErrorMessage = "Room number is too long.")]
        [Display(Name = "Room (Enter 0 for Out-of-House)")]
        public string Room { get; set; }
        
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

    public class AppointmentModel
    {
        public Leader Leader { get; set; } 
        public IEnumerable<object> Users { get; set; }
    }
    
    public class AccountInformationModel
    {
        public Member MemberInfo { get; set; }
        public LocalPasswordModel ChangePasswordModel { get; set; }
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
