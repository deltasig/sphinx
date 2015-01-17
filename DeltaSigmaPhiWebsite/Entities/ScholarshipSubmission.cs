namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScholarshipSubmission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ScholarshipSubmissionId { get; set; }
        public int ScholarshipAppId { get; set; }

        public bool IsWinner { get; set; }

        public DateTime SubmittedOn { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        public string StudentNumber { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(([0-9]{3}[-]([0-9]{3})[-][0-9]{4})|([+]?[0-9]{10,15}))$",
            ErrorMessage = "Phone number format was invalid (enter US as ###-###-#### or international as +1 followed by up to 15 digits).")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "An email address is required")]
        [StringLength(50)]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [StringLength(2)]
        public string State { get; set; }

        public int PostalCode { get; set; }

        [Required]
        [StringLength(50)]
        public string Country { get; set; }

        [Required]
        [StringLength(100)]
        public string HighSchool { get; set; }

        [Required]
        [Range(0, 2400)]
        public int ActSatScore { get; set; }

        [Required]
        [Range(0, 10)]
        public double Gpa { get; set; }

        [Required]
        [StringLength(100)]
        public string HearAboutScholarship { get; set; }

        [ForeignKey("ScholarshipAppId")]
        public virtual ScholarshipApp Application { get; set; }
        public virtual ICollection<ScholarshipAnswer> Answers { get; set; }
    }
}