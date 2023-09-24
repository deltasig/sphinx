using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dsp.Data.Entities;

public partial class User : IdentityUser<int>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public int StatusId { get; set; }

    public int? PledgeClassId { get; set; }

    public int? ExpectedGraduationId { get; set; }

    public int? BigBroId { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? LastUpdatedOn { get; set; }

    public string AvatarPath { get; set; }

    public string DietaryInstructions { get; set; }

    public string EmergencyContact { get; set; }

    public string EmergencyRelation { get; set; }

    public string EmergencyPhoneNumber { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual User BigBro { get; set; }

    public virtual ICollection<ClassTaken> ClassesTaken { get; set; } = new List<ClassTaken>();

    public virtual Semester ExpectedGraduation { get; set; }

    public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();

    public virtual ICollection<User> LittleBros { get; set; } = new List<User>();

    public virtual ICollection<LaundrySignup> LaundrySignups { get; set; } = new List<LaundrySignup>();

    public virtual ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

    public virtual ICollection<MajorToMember> Majors { get; set; } = new List<MajorToMember>();

    public virtual ICollection<MealItemVote> MealItemVotes { get; set; } = new List<MealItemVote>();

    public virtual ICollection<MealPlate> MealPlates { get; set; } = new List<MealPlate>();

    public virtual PledgeClass PledgeClass { get; set; }

    public virtual ICollection<RoomToMember> Rooms { get; set; } = new List<RoomToMember>();

    public virtual ICollection<ServiceEventAmendment> ServiceEventAmendments { get; set; } = new List<ServiceEventAmendment>();

    public virtual ICollection<ServiceHourAmendment> ServiceHourAmendments { get; set; } = new List<ServiceHourAmendment>();

    public virtual ICollection<ServiceEvent> ServiceEvents { get; set; } = new List<ServiceEvent>();

    public virtual ICollection<ServiceHour> ServiceHours { get; set; } = new List<ServiceHour>();

    public virtual ICollection<SoberSignup> SoberSignups { get; set; } = new List<SoberSignup>();

    public virtual UserType Status { get; set; }

    public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();

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
