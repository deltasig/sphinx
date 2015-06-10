namespace Dsp.Entities
{
    using Data;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class Member : IdentityUser<int, SphinxUserLogin, Leader, SphinxUserClaim>
    {
        [Required, StringLength(100), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, StringLength(50), Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Range(0, 999999999, ErrorMessage = "Pin number is too long."), DataType(DataType.Text)]
        public int? Pin { get; set; }

        [Range(0, 999, ErrorMessage = "Room number is too long."), DataType(DataType.Text)]
        [Display(Name = "Room (Enter 0 for Out-of-House)")]
        public int? Room { get; set; }

        [Required, Display(Name = "Member Status")]
        public int StatusId { get; set; }

        [Display(Name = "Pledge Class")]
        public int? PledgeClassId { get; set; }

        [Display(Name = "Graduation")]
        public int? ExpectedGraduationId { get; set; }

        [Display(Name = "Big Brother")]
        public int? BigBroId { get; set; }

        [Display(Name = "Shirt Size")]
        public string ShirtSize { get; set; }

        [ForeignKey("StatusId")]
        public virtual MemberStatus MemberStatus { get; set; }
        [ForeignKey("PledgeClassId")]
        public virtual PledgeClass PledgeClass { get; set; }
        [ForeignKey("ExpectedGraduationId")]
        public virtual Semester GraduationSemester { get; set; }
        [ForeignKey("BigBroId")]
        public virtual Member BigBrother { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<Address> Addresses { get; set; }
        [InverseProperty("Uploader")]
        public virtual ICollection<ClassFile> ClassFileUploads { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<ClassFileVote> ClassFileVotes { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<ClassTaken> ClassesTaken { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<IncidentReport> IncidentReports { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<LaundrySignup> LaundrySignups { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<Leader> Leaders { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<MajorToMember> MajorsToMember { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<MealVote> MealVotes { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<MealLatePlate> MealLatePlates { get; set; }
        [InverseProperty("BigBrother")]
        public virtual ICollection<Member> LittleBrothers { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<RoomToMember> Rooms { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<ServiceHour> ServiceHours { get; set; }
        [InverseProperty("Submitter")]
        public virtual ICollection<Event> SubmittedEvents { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<SoberSignup> SoberSignups { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<WorkOrder> WorkOrders { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<WorkOrderComment> WorkOrderComments { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<WorkOrderPriorityChange> WorkOrderPriorityChanges { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<WorkOrderStatusChange> WorkOrderStatusChanges { get; set; }

        public string RoomString()
        {
            if (Room == null)
                return "Unassigned";
            return Room == 0 ? "OOH" : Room.ToString();
        }
        
        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<Member, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }
}