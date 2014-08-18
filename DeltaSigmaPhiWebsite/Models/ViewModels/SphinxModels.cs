﻿namespace DeltaSigmaPhiWebsite.Models.ViewModels
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
        public IEnumerable<string> Roles { get; set; }
        public string ProfilePicUrl { get; set; }
        public string LaundrySummary { get; set; }
        public IEnumerable<StudyHour> StudyHours { get; set; }
        public IEnumerable<StudyHour> StudyApproval { get; set; }
        public StudyHourSubmissionModel StudyModel { get; set; }
        public IEnumerable<ServiceHour> CompletedEvents { get; set; }
        public IEnumerable<SoberSignup> SoberSignups { get; set; }
    }

    public class StudyHourSubmissionModel
    {
        [Required]
        [Display(Name = "Hours")]
        [DataType(DataType.Duration)]
        public int HoursStudied { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime DateTimeStudied { get; set; }

        [Required]
        [Display(Name = "Approver")]
        [DataType(DataType.Text)]
        public int SelectedApproverId { get; set; }
        public IEnumerable<SelectListItem> Approvers { get; set; }
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

    public class ServiceHourCompletedModel
    {
        public string EventName { get; set; }
        public int HoursComplete { get; set; }
        public bool Approval { get; set; }
    }

    public class SoberManagerModel
    {
        public IEnumerable<SoberSignup> VacantSignups { get; set; }

        [Required]
        [Display(Name = "Date Driver is Needed")]
        [DataType(DataType.Date)]
        public DateTime SoberDriverReservationRequest { get; set; }

        [Required]
        [Display(Name = "Date Officer is Needed")]
        [DataType(DataType.Date)]
        public DateTime SoberOfficerReservationRequest { get; set; }
    }
    
    public class LaundrySignupModel
    {
        public List< List <LaundryReservationModel> > ThisWeeksSignups { get; set; }
    }

    public class LaundryReservationModel
    {
        public int UserId {get; set;}
        public string Name { get; set; }
        public string UserName { get; set; }
        public DateTime Shift { get; set; }
    }

    public class ServiceIndexModel
    {
        public Member Member { get; set; }
        public double Hours { get; set; }
        public Semester Semester { get; set; }
    }
}

