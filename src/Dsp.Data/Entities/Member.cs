namespace Dsp.Data.Entities
{
    using Data;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
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

        public DateTime? CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public string AvatarPath { get; set; }

        [StringLength(50), Display(Name = "Dietary Instructions")]
        public string DietaryInstructions { get; set; }

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
        [InverseProperty("Member")]
        public virtual ICollection<BugReport> BugReports { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<ChoreGroupToMember> ChoreGroups { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<ChoreAssignment> ChoreSignOffs { get; set; }
        [InverseProperty("Enforcer")]
        public virtual ICollection<ChoreAssignment> ChoresEnforced { get; set; }
        [InverseProperty("Uploader")]
        public virtual ICollection<ClassFile> ClassFileUploads { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<ClassTaken> ClassesTaken { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<IncidentReport> IncidentReports { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<LaundrySignup> LaundrySignups { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<Leader> PositionsHeld { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<MajorToMember> MajorsToMember { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<MealItemVote> MealItemVotes { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<MealPlate> MealPlates { get; set; }
        [InverseProperty("BigBrother")]
        public virtual ICollection<Member> LittleBrothers { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<RoomToMember> Rooms { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<ServiceHourAmendment> ServiceHourAmendments { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<ServiceEventAmendment> ServiceEventAmendments { get; set; }
        [InverseProperty("Member")]
        public virtual ICollection<ServiceHour> ServiceHours { get; set; }
        [InverseProperty("Submitter")]
        public virtual ICollection<ServiceEvent> SubmittedEvents { get; set; }
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

        public RoomToMember GetMostRecentRoomAssignment()
        {
            return Rooms.OrderByDescending(r => r.MovedOut).FirstOrDefault();
        }

        public string RoomString()
        {
            if (!Rooms.Any()) return "No rooms found";

            var mostRecentAssignment = GetMostRecentRoomAssignment();
            if (mostRecentAssignment.MovedOut >= DateTime.UtcNow)
            {
                return mostRecentAssignment.Room.Name;
            }
            return mostRecentAssignment.Room.Name + " (OOD)";
        }

        public bool HasLivingAssignment(int sid)
        {
            return Rooms.Any(r => r.Room.SemesterId == sid);
        }

        public bool IsLivingInHouse()
        {
            var mostRecentAssignment = GetMostRecentRoomAssignment();
            if (mostRecentAssignment == null) return false;

            int value;
            return int.TryParse(mostRecentAssignment.Room.Name, out value);
        }

        public bool WasLivingInHouse(DateTime dateTime)
        {
            var livingSituationOnGivenDate = Rooms
                .SingleOrDefault(r => r.MovedIn <= dateTime && dateTime <= r.MovedOut);
            if (livingSituationOnGivenDate == null) return false;

            int value;
            return int.TryParse(livingSituationOnGivenDate.Room.Name, out value);
        }

        public bool WasLivingInHouse(int sid)
        {
            var assignmentsDuringSemester = Rooms.Where(r => r.Room.SemesterId == sid).ToList();
            if (!assignmentsDuringSemester.Any()) return false;

            foreach (var a in assignmentsDuringSemester)
            {
                int value;
                if (int.TryParse(a.Room.Name, out value))
                    return true;
            }

            return false;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<Member, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }

        public string LivingAssignmentForSemester(int sid)
        {
            if (!HasLivingAssignment(sid))
            {
                return "Unknown";
            }

            var assignments = Rooms.Where(r => r.Room.SemesterId == sid).OrderByDescending(r => r.MovedOut);

            return assignments.First().Room.Name;
        }

        public string GetAvatarString()
        {
            var filePath = "";//AccountController.ImageUpload.GetUploadPath(AvatarPath ?? string.Empty);
            var fileExists = System.IO.File.Exists(filePath);
            return fileExists ? AvatarPath : "NoAvatar.jpg";
        }

        public string ToShortLastNameString()
        {
            return FirstName + " " + LastName.Substring(0, 1) + ".";
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }
}