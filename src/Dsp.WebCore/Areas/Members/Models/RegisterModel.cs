namespace Dsp.WebCore.Areas.Members.Models
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class RegisterModel
    {

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "The email address is required")]
        [StringLength(50)]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
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

        [DataType(DataType.Text)]
        [Display(Name = "Graduation")]
        public string ExpectedGraduationId { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Pledge Class")]
        public string PledgeClassId { get; set; }
        public IEnumerable<SelectListItem> PledgeClassList { get; set; }
    }
}