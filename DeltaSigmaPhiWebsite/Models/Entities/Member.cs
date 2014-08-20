namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Member
    {
        public Member()
        {
            Addresses = new HashSet<Address>();
            ClassesTaken = new HashSet<ClassTaken>();
            IncidentReports = new HashSet<IncidentReport>();
            LaundrySignups = new HashSet<LaundrySignup>();
            Leaders = new HashSet<Leader>();
            LittleBrothers = new HashSet<Member>();
            OrganizationsJoined = new HashSet<OrganizationsJoined>();
            PhoneNumbers = new HashSet<PhoneNumber>();
            ServiceHours = new HashSet<ServiceHour>();
            SoberSignups = new HashSet<SoberSignup>();
            SubmittedStudyHours = new HashSet<StudyHour>();
            ApprovedStudyHours = new HashSet<StudyHour>();
            webpages_OAuthMembership = new HashSet<webpages_OAuthMembership>();
            Committees = new HashSet<Leader>();
            Majors = new HashSet<Major>();
        }

        [Key]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Email { get; set; }

        [Range(0, 9999999999, ErrorMessage = "Pin number is too long.")]
        [DataType(DataType.Text)]
        public int? Pin { get; set; }

        [Range(0, 999, ErrorMessage = "Room number is too long.")]
        [DataType(DataType.Text)]
        [Display(Name = "Room (Enter 0 for Out-of-House)")]
        public int? Room { get; set; }

        public double? PreviousSemesterGPA { get; set; }

        public double? CumulativeGPA { get; set; }

        public double? RemainingBalance { get; set; }

        [Range(0, 12, ErrorMessage = "Please enter a number from 0-12")]
        public int RequiredStudyHours { get; set; }

        [Range(0, 12, ErrorMessage = "Please enter a number from 0-12")]
        public int? ProctoredStudyHours { get; set; }

        public int StatusId { get; set; }

        public int PledgeClassId { get; set; }

        public int ExpectedGraduationId { get; set; }

        public int? BigBroId { get; set; }
        
        public virtual Member BigBrother { get; set; }

        public virtual MemberStatus MemberStatus { get; set; }

        public virtual PledgeClass PledgeClass { get; set; }

        public virtual Semester Semester { get; set; }

        public virtual webpages_Membership webpages_Membership { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }

        public virtual ICollection<ClassTaken> ClassesTaken { get; set; }

        public virtual ICollection<IncidentReport> IncidentReports { get; set; }

        public virtual ICollection<LaundrySignup> LaundrySignups { get; set; }

        public virtual ICollection<Leader> Leaders { get; set; }

        public virtual ICollection<Member> LittleBrothers { get; set; }

        public virtual ICollection<OrganizationsJoined> OrganizationsJoined { get; set; }

        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }

        public virtual ICollection<ServiceHour> ServiceHours { get; set; }

        public virtual ICollection<SoberSignup> SoberSignups { get; set; }

        public virtual ICollection<StudyHour> SubmittedStudyHours { get; set; }
        public virtual ICollection<StudyHour> ApprovedStudyHours { get; set; }

        public virtual ICollection<webpages_OAuthMembership> webpages_OAuthMembership { get; set; }
        
        public virtual ICollection<Leader> Committees { get; set; }

        public virtual ICollection<Major> Majors { get; set; }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

        public string RoomString()
        {
            if (Room == null)
                return "Unassigned";
            return Room == 0 ? "Out-of-House" : Room.ToString();
        }
    }
}
