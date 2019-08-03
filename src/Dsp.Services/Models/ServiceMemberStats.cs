using System;

namespace Dsp.Services.Models
{
    public class ServiceMemberStats
    {
        public int NonExemptMemberCount { get; }
        public int ExemptMemberCount { get; }
        public int NonExemptActiveMemberCount { get; }
        public int NonExemptNewMemberCount { get; }
        public double TotalMemberHours { get; }
        public double TotalActiveHours { get; }
        public double TotalNewMemberHours { get; }
        public int AllMembersServed { get; }
        public int AllMembersServedPercentage { get; }
        public int ActiveMembersServed { get; }
        public int ActiveMembersServedPercentage { get; }
        public int NewMembersServed { get; }
        public int NewMembersServedPercentage { get; }
        public int AllMembersServedTen { get; }
        public int AllMembersServedTenPercentage { get; }
        public int ActiveMembersServedTen { get; }
        public int ActiveMembersServedTenPercentage { get; }
        public int NewMembersServedTen { get; }
        public int NewMembersServedTenPercentage { get; }
        public int AllMembersServedFifteen { get; }
        public int AllMembersServedFifteenPercentage { get; }
        public int ActiveMembersServedFifteen { get; }
        public int ActiveMembersServedFifteenPercentage { get; }
        public int NewMembersServedFifteen { get; }
        public int NewMembersServedFifteenPercentage { get; }
        public string AverageAllMemberHours { get; }
        public string AverageActiveMemberHours { get; }
        public string AverageNewMemberHours { get; }
        public int AverageAllMemberAttendance { get; }
        public int AverageAllMemberAttendancePercentage { get; }
        public int AverageActiveMemberAttendance { get; }
        public int AverageActiveMemberAttendancePercentage { get; }
        public int AverageNewMemberAttendance { get; }
        public int AverageNewMemberAttendancePercentage { get; }
        public string ExemptMembersDisplay { get; }
        public DateTime CalculatedOn { get; }

        public ServiceMemberStats(
            int nonExemptMemberCount,
            int exemptMemberCount,
            int nonExemptActiveMemberCount,
            int nonExemptNewMemberCount,
            double totalAllMemberHours,
            double totalActiveMemberHours,
            double totalNewMemberHours,
            int allMembersServed,
            int allMembersServedPercentage,
            int activeMembersServed,
            int activeMembersServedPercentage,
            int newMembersServed,
            int newMembersServedPercentage,
            int allMembersServedTen,
            int allMembersServedTenPercentage,
            int activeMembersServedTen,
            int activeMembersServedTenPercentage,
            int newMembersServedTen,
            int newMembersServedTenPercentage,
            int allMembersServedFifteen,
            int allMembersServedFifteenPercentage,
            int activeMembersServedFifteen,
            int activeMembersServedFifteenPercentage,
            int newMembersServedFifteen,
            int newMembersServedFifteenPercentage,
            string averageAllMemberHours,
            string averageActiveMemberHours,
            string averageNewMemberHours,
            int averageAllMemberAttendance,
            int averageAllMemberAttendancePercentage,
            int averageActiveMemberAttendance,
            int averageActiveMemberAttendancePercentage,
            int averageNewMemberAttendance,
            int averageNewMemberAttendancePercentage,
            string exemptMembersDisplay,
            DateTime calculatedOn)
        {
            NonExemptMemberCount = nonExemptMemberCount;
            ExemptMemberCount = exemptMemberCount;
            NonExemptActiveMemberCount = nonExemptActiveMemberCount;
            NonExemptNewMemberCount = nonExemptNewMemberCount;
            TotalMemberHours = totalAllMemberHours;
            TotalActiveHours = totalActiveMemberHours;
            TotalNewMemberHours = totalNewMemberHours;
            AllMembersServed = allMembersServed;
            AllMembersServedPercentage = allMembersServedPercentage;
            ActiveMembersServed = activeMembersServed;
            ActiveMembersServedPercentage = activeMembersServedPercentage;
            NewMembersServed = newMembersServed;
            NewMembersServedPercentage = newMembersServedPercentage;
            AllMembersServedTen = allMembersServedTen;
            AllMembersServedTenPercentage = allMembersServedTenPercentage;
            ActiveMembersServedTen = activeMembersServedTen;
            ActiveMembersServedTenPercentage = activeMembersServedTenPercentage;
            NewMembersServedTen = newMembersServedTen;
            NewMembersServedTenPercentage = newMembersServedTenPercentage;
            AllMembersServedFifteen = allMembersServedFifteen;
            AllMembersServedFifteenPercentage = allMembersServedFifteenPercentage;
            ActiveMembersServedFifteen = activeMembersServedFifteen;
            ActiveMembersServedFifteenPercentage = activeMembersServedFifteenPercentage;
            NewMembersServedFifteen = newMembersServedFifteen;
            NewMembersServedFifteenPercentage = newMembersServedFifteenPercentage;
            AverageAllMemberHours = averageAllMemberHours;
            AverageActiveMemberHours = averageActiveMemberHours;
            AverageNewMemberHours = averageNewMemberHours;
            AverageAllMemberAttendance = averageAllMemberAttendance;
            AverageAllMemberAttendancePercentage = averageAllMemberAttendancePercentage;
            AverageActiveMemberAttendance = averageActiveMemberAttendance;
            AverageActiveMemberAttendancePercentage = averageActiveMemberAttendancePercentage;
            AverageNewMemberAttendance = averageNewMemberAttendance;
            AverageNewMemberAttendancePercentage = averageNewMemberAttendancePercentage;
            ExemptMembersDisplay = exemptMembersDisplay;
            CalculatedOn = calculatedOn;
        }
    }
}
