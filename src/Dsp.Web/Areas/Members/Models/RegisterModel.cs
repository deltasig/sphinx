namespace Dsp.Web.Areas.Members.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

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
        
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Graduation")]
        public string ExpectedGraduationId { get; set; }
        public IEnumerable<SelectListItem> SemesterList { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Pledge Class")]
        public string PledgeClassId { get; set; }
        public IEnumerable<SelectListItem> PledgeClassList { get; set; }
        
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Shirt Size")]
        public string ShirtSize { get; set; }
        public IEnumerable<SelectListItem> ShirtSizes { get; set; }
    }
}