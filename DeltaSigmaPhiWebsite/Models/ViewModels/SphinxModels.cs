namespace DeltaSigmaPhiWebsite.Models.ViewModels
{
    using Entities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class SphinxModel
    {
        public Member MemberInfo { get; set; }
        public bool NeedsToSoberDrive { get; set; }
        public int RemainingStudyHours { get; set; }
        public int RemainingCommunityServiceHours { get; set; }
        public IEnumerable<LaundrySignup> LaundrySummary { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<StudyHour> StudyHours { get; set; }
        public IEnumerable<ServiceHour> CompletedEvents { get; set; }
        public IEnumerable<SoberSignup> SoberSignups { get; set; }
    }
    
    public class ServiceHourSubmissionModel
    {
        [Required]
        [Display(Name = "Event")]
        [DataType(DataType.Text)]
        public int SelectedEventId { get; set; }
        public IEnumerable<SelectListItem> Events { get; set; }

        [Required]
        [Display(Name = "Hours")]
        [DataType(DataType.Duration)]
        public int HoursServed { get; set; }
        public DateTime SoberDriveTime { get; set; }
    }
  
    public class ServiceIndexModel
    {
        public Member Member { get; set; }
        public double Hours { get; set; }
        public Semester Semester { get; set; }
    }
}

