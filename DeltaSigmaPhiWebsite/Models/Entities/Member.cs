namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

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
            SoberDrivers = new HashSet<SoberDriver>();
            SoberOfficers = new HashSet<SoberOfficer>();
            StudyHours = new HashSet<StudyHour>();
            webpages_OAuthMembership = new HashSet<webpages_OAuthMembership>();
            Leaders1 = new HashSet<Leader>();
            IncidentReports1 = new HashSet<IncidentReport>();
            Majors = new HashSet<Major>();
            webpages_Roles = new HashSet<webpages_Roles>();
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

        [StringLength(50)]
        public string Nickname { get; set; }

        [Required]
        [StringLength(50)]
        public string Email { get; set; }

        public int? Pin { get; set; }

        public int? Room { get; set; }

        public double? PreviousSemesterGPA { get; set; }

        public double? CumulativeGPA { get; set; }

        public double? RemainingBalance { get; set; }

        public int? StatusId { get; set; }

        public int? PledgeClassId { get; set; }

        public int? ExpectedGraduationId { get; set; }

        public int? BigBroId { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }

        public virtual ICollection<ClassTaken> ClassesTaken { get; set; }

        public virtual ICollection<IncidentReport> IncidentReports { get; set; }

        public virtual ICollection<LaundrySignup> LaundrySignups { get; set; }

        public virtual ICollection<Leader> Leaders { get; set; }

        public virtual ICollection<Member> LittleBrothers { get; set; }

        public virtual Member BigBrother { get; set; }

        public virtual MemberStatus MemberStatus { get; set; }

        public virtual ICollection<OrganizationsJoined> OrganizationsJoined { get; set; }

        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }

        public virtual PledgeClass PledgeClass { get; set; }

        public virtual ICollection<ServiceHour> ServiceHours { get; set; }

        public virtual ICollection<SoberDriver> SoberDrivers { get; set; }

        public virtual ICollection<SoberOfficer> SoberOfficers { get; set; }

        public virtual ICollection<StudyHour> StudyHours { get; set; }

        public virtual Semester Semester { get; set; }

        public virtual webpages_Membership webpages_Membership { get; set; }

        public virtual ICollection<webpages_OAuthMembership> webpages_OAuthMembership { get; set; }
        
        public virtual ICollection<Leader> Leaders1 { get; set; }

        public virtual ICollection<IncidentReport> IncidentReports1 { get; set; }

        public virtual ICollection<Major> Majors { get; set; }

        public virtual ICollection<webpages_Roles> webpages_Roles { get; set; }
    }
}
