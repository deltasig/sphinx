namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using System.Linq;       //IF YOU REMOVE "CustomValidation" region THEN REMOVE THIS

    public class SphinxModel
    {
        public Member MemberInfo { get; set; }
        public int RemainingStudyHours { get; set; }
        public int RemainingCommunityServiceHours { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public string ProfilePicUrl { get; set; }
        public string LaundrySummary { get; set; }
        public IEnumerable<StudyHour> StudyHours { get; set; }
        public IEnumerable<StudyHour> StudyApproval { get; set; }
        public StudyHourSubmissionModel StudyModel { get; set; }
        public ServiceHourSubmissionModel ServiceModel { get; set; }
        public List<SoberReservationModel> SoberSignedUp { get; set; }
        public List<SoberReservationModel> FullSoberSchedule { get; set; }
        public ChoresToDo AllChores { get; set; }
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

        public IEnumerable<ServiceHourCompletedModel> CompletedEvents { get; set; }

        public DateTime SoberDriveTime { get; set; }
    }

    public class ServiceHourCompletedModel
    {
        public string EventName { get; set; }
        public int HoursComplete { get; set; }
        public bool Approval { get; set; }
    }

    public class AdminPanelModel
    {
        public AddSemesterModel SemesterModel { get; set; }
        public MyChoreModel NewChoreModel { get; set; }
    }

    public class MyChoreModel
    {
        [Required(ErrorMessage = "Chore Requires a Name")]
        [DataType(DataType.Text)]
        [StringLength(50,ErrorMessage="Description cannot be longer than 100 characters")]
        [Display(Name = "Chore Name")]
        public string ChoreName { get; set; }

        [Required(ErrorMessage = "Please give a simple description of the chore")]
        [DataType(DataType.Text)]
        [StringLength(100, ErrorMessage="Description cannot be longer than 100 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please give all neccessary information for the chore")]
        [DataType(DataType.Text)]
        [Display(Name = "Direction")]
        public string Direction { get; set; }

        [Required(ErrorMessage = "All Chores require a type")]
        [DataType(DataType.Text)]
        [Display(Name = "Chore Type")]
        public int selectedChoreType { get; set; }
        public IEnumerable<SelectListItem> ChoreTypes { get; set; }

        [Required(ErrorMessage = "All Chores require a class")]
        [Display(Name = "Chore Class")]
        public int selectedChoreClass { get; set; }
        public IEnumerable<SelectListItem> ChoreClasses { get; set; }

        [ChoreDateDue("selectedChoreType", ErrorMessage = "For your Chore Type, a compltion date is required")]
        [DataType(DataType.DateTime)]
        [ValidChoreDate("selectedChoreType", ErrorMessage = "You must enter a date/time that is in the future")]
        [Display(Name = "Due Date")]
        public DateTime CompletedBy { get; set; }

        [AssignedMembersList("selectedChoreType", ErrorMessage = "For your Chore Type, at least one person must be assigned to the chore")]
        [Display(Name = "Assigned To Chore")]
        public IEnumerable<int> AssignedToChore { get; set; }
        public MultiSelectList AllMemebers { get; set; }

        [WeekDaysAttribute(new string[1] { "selectedChoreType" }, new string[6] { "OnMonday", "OnTuesday", "OnWednesday", "OnThursday", "OnFriday", "OnSaturday" }, ErrorMessage = "For your Chore Type, at least one day must be selected")]
        [Display(Name = "Sunday")]
        public bool OnSunday { get; set; }

        [Display(Name = "Monday")]
        public bool OnMonday { get; set; }

        [Display(Name = "Tuesday")]
        public bool OnTuesday { get; set; }

        [Display(Name = "Wednesday")]
        public bool OnWednesday { get; set; }

        [Display(Name = "Thursday")]
        public bool OnThursday { get; set; }

        [Display(Name = "Friday")]
        public bool OnFriday { get; set; }

        [Display(Name = "Saturday")]
        public bool OnSaturday { get; set; }
    }

    public class AddSemesterModel
    {
        [Required]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required]
        [Display(Name = "Term")]
        [DataType(DataType.Text)]
        public string SelectedTerm { get; set; }
        public IEnumerable<SelectListItem> Terms { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }

    public class SoberSignupModel
    {
        public List<SoberReservationModel> SoberSignUpsNeededList { get; set; }

        [Required]
        [Display(Name = "Date Driver is Needed")]
        [DataType(DataType.Date)]
        public DateTime SoberDriverReservationRequest { get; set; }

        [Required]
        [Display(Name = "Date Officer is Needed")]
        [DataType(DataType.Date)]
        public DateTime SoberOfficerReservationRequest { get; set; }
    }

    public class SoberReservationModel 
    {
        public int? UID { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public DateTime Shift { get; set; }
        public string SoberType { get; set; }
    }

    public class IncidentReportModel
    {
        public int? RepBy { get; set; }
        public IEnumerable<IncidentReport> RecentIncidentReports { get; set; }
            
        [Required]
        [Display(Name = "Date of Incident")]
        [DataType(DataType.Date)]
        public DateTime IncidentDate { get; set; }

        [Required]
        [Display(Name = "Member Responsible")]
        [DataType(DataType.Text)]
        public string MemberBeingReported { get; set; }
        public IEnumerable<SelectListItem> Members { get; set; }

        [Required]
        [Display(Name = "Behavior Witnessed")]
        [DataType(DataType.MultilineText)]
        public string BehaviorsWitnessed { get; set; }

        [Required]
        [Display(Name = "Policy Broken")]
        [DataType(DataType.Text)]
        public string PolicyBroken { get; set; }
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

    public class ChoresToDo
    {
        public List<SingleChore> AllUsersChores { get; set; }
    }

    public class SingleChore
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public DateTime CompletedBy { get; set; }
        public string Description { get; set; }
        public string Directions { get; set; }
        public bool Status {get; set;}
}

    public class MyTestModel
    {
        public string Gender { get; set; }
        public string Title { get; set; }
        public string TmpTitle { get; set; }
}

    #region CustomValidation

    public class ChoreDateDueAttribute : ValidationAttribute
    {
        public ChoreDateDueAttribute(params string[] ChoreType)
        {
            this.PropertyName = ChoreType;
        }
        public string[] PropertyName { get; private set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var properties = this.PropertyName.Select(validationContext.ObjectType.GetProperty);
            var values = properties.Select(p => p.GetValue(validationContext.ObjectInstance, null)).OfType<int>();
            if (values.FirstOrDefault() != 4 && value == null)
            {
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            }
            return ValidationResult.Success;
        }
    }

    public class AssignedMembersListAttribute : ValidationAttribute
    {
        public AssignedMembersListAttribute(params string[] ChoreType)
        {
            this.PropertyName = ChoreType;
        }
        public string[] PropertyName { get; private set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var properties = this.PropertyName.Select(validationContext.ObjectType.GetProperty);
            var values = properties.Select(p => p.GetValue(validationContext.ObjectInstance, null)).OfType<int>();
            if (values.FirstOrDefault() != 4 && value == null)
            {
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            }
            return ValidationResult.Success;
        }
    }

    public class ValidChoreDate : ValidationAttribute
    {
        public ValidChoreDate(params string[] ChoreType)
        {
            this.PropertyName = ChoreType;
        }
        public string[] PropertyName { get; private set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var properties = this.PropertyName.Select(validationContext.ObjectType.GetProperty);
            var values = properties.Select(p => p.GetValue(validationContext.ObjectInstance, null)).OfType<int>();

            if (values.FirstOrDefault() != 4 && value != null)
            {
                if ((DateTime)value < DateTime.Now)
                    return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
                else
                    return ValidationResult.Success;
            }
            return ValidationResult.Success;
        }
    }

    public class WeekDaysAttribute : ValidationAttribute
    {
        public WeekDaysAttribute(string[] ChoreType, string[] BoolProperties)
        {

            this.BoolPropertyNames = BoolProperties;
            this.PropertyName = ChoreType;
        }

        public string[] PropertyName { get; private set; }
        public string[] BoolPropertyNames { get; private set; }

        protected override ValidationResult IsValid(object inputValue, ValidationContext validationContext)
        {
            var properties = this.PropertyName.Select(validationContext.ObjectType.GetProperty);
            var values = properties.Select(p => p.GetValue(validationContext.ObjectInstance, null)).OfType<int>();

            var boolProperties = this.BoolPropertyNames.Select(validationContext.ObjectType.GetProperty);
            var boolValues = boolProperties.Select(p => p.GetValue(validationContext.ObjectInstance, null)).ToList();

            if(values.FirstOrDefault() == 4)
            {
                foreach (bool value in boolValues)
                {
                    if(value == true)
                    {
                        return ValidationResult.Success;
                    }
                }
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            }
            return ValidationResult.Success;
        }
    }

    #endregion
    
}

