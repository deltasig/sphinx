namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Member
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "The email address is required")]
        [StringLength(50)]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Range(0, 999999999, ErrorMessage = "Pin number is too long.")]
        [DataType(DataType.Text)]
        public int? Pin { get; set; }

        [Range(0, 999, ErrorMessage = "Room number is too long.")]
        [DataType(DataType.Text)]
        [Display(Name = "Room (Enter 0 for Out-of-House)")]
        public int? Room { get; set; }
        
        [Display(Name = "Member Status")]
        public int StatusId { get; set; }

        [Display(Name = "Pledge Class")]
        public int PledgeClassId { get; set; }

        [Display(Name = "Graduation")]
        public int ExpectedGraduationId { get; set; }

        [Display(Name = "Big Brother")]
        public int? BigBroId { get; set; }

        [Display(Name = "Shirt Size")]
        public string ShirtSize { get; set; }

        public virtual Member BigBrother { get; set; }

        public virtual MemberStatus MemberStatus { get; set; }

        public virtual PledgeClass PledgeClass { get; set; }

        public virtual Semester GraduationSemester { get; set; }

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

        public virtual ICollection<Event> SubmittedEvents { get; set; }

        public virtual ICollection<SoberSignup> SoberSignups { get; set; }

        public virtual ICollection<StudyHour> StudyHourApprovals { get; set; }

        public virtual ICollection<StudyAssignment> StudyHourAssignments { get; set; }

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