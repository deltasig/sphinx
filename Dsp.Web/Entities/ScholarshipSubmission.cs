namespace Dsp.Web.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScholarshipSubmission
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ScholarshipSubmissionId { get; set; }

        public int ScholarshipAppId { get; set; }

        public bool IsWinner { get; set; }

        public DateTime SubmittedOn { get; set; }

        [Required, StringLength(50), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, StringLength(50), Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required, Display(Name = "Student Number"), StringLength(15)]
        public string StudentNumber { get; set; }

        [Required, DataType(DataType.PhoneNumber), Display(Name = "Phone Number"), StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "An email address is required"), StringLength(50), Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required, StringLength(100),Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [Required, StringLength(50), Display(Name = "City")]
        public string City { get; set; }

        [StringLength(2), Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "Postal Code")]
        public int PostalCode { get; set; }

        [Required, StringLength(50), Display(Name = "Country")]
        public string Country { get; set; }

        [Required, StringLength(100), Display(Name = "High School")]
        public string HighSchool { get; set; }

        [Required, Range(0, 2400), Display(Name = "ACT or SAT")]
        public int ActSatScore { get; set; }

        [Required, Range(0, 5), Display(Name = "GPA")]
        public double Gpa { get; set; }

        [Required, StringLength(100), Display(Name = "How Did You Heard About Us")]
        public string HearAboutScholarship { get; set; }

        [Display(Name = "Committee Response"), DataType(DataType.MultilineText)]
        public string CommitteeResponse { get; set; }

        public DateTime? CommitteeRespondedOn { get; set; }

        [ForeignKey("ScholarshipAppId")]
        public virtual ScholarshipApp Application { get; set; }
        [InverseProperty("Submission")]
        public virtual ICollection<ScholarshipAnswer> Answers { get; set; }
    }
}