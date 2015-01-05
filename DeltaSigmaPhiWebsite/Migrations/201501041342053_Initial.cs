namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        AddressId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Type = c.String(nullable: false, maxLength: 20),
                        Address1 = c.String(maxLength: 100),
                        Address2 = c.String(maxLength: 100),
                        City = c.String(maxLength: 50),
                        State = c.String(maxLength: 2),
                        PostalCode = c.Int(nullable: false),
                        Country = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.AddressId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Members",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 50),
                        Pin = c.Int(),
                        Room = c.Int(),
                        PreviousSemesterGPA = c.Double(),
                        CumulativeGPA = c.Double(),
                        RemainingBalance = c.Double(),
                        RequiredStudyHours = c.Int(nullable: false),
                        ProctoredStudyHours = c.Int(),
                        StatusId = c.Int(nullable: false),
                        PledgeClassId = c.Int(nullable: false),
                        ExpectedGraduationId = c.Int(nullable: false),
                        BigBroId = c.Int(),
                        ShirtSize = c.String(),
                        webpages_Roles_RoleId = c.Int(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Semesters", t => t.ExpectedGraduationId, cascadeDelete: true)
                .ForeignKey("dbo.PledgeClasses", t => t.PledgeClassId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.BigBroId)
                .ForeignKey("dbo.MemberStatus", t => t.StatusId, cascadeDelete: true)
                .ForeignKey("dbo.webpages_Roles", t => t.webpages_Roles_RoleId)
                .Index(t => t.StatusId)
                .Index(t => t.PledgeClassId)
                .Index(t => t.ExpectedGraduationId)
                .Index(t => t.BigBroId)
                .Index(t => t.webpages_Roles_RoleId);
            
            CreateTable(
                "dbo.ClassesTaken",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        ClassId = c.Int(nullable: false),
                        SemesterId = c.Int(nullable: false),
                        Instructor = c.String(maxLength: 40),
                        MidtermGrade = c.String(maxLength: 1, fixedLength: true, unicode: false),
                        FinalGrade = c.String(maxLength: 1, fixedLength: true, unicode: false),
                        Dropped = c.Boolean(),
                    })
                .PrimaryKey(t => new { t.UserId, t.ClassId, t.SemesterId })
                .ForeignKey("dbo.Classes", t => t.ClassId)
                .ForeignKey("dbo.Semesters", t => t.SemesterId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.ClassId)
                .Index(t => t.SemesterId);
            
            CreateTable(
                "dbo.Classes",
                c => new
                    {
                        ClassId = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(nullable: false),
                        CourseShorthand = c.String(nullable: false, maxLength: 15),
                        CourseName = c.String(nullable: false, maxLength: 100),
                        CreditHours = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ClassId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        DepartmentId = c.Int(nullable: false, identity: true),
                        DepartmentName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Majors",
                c => new
                    {
                        MajorId = c.Int(nullable: false, identity: true),
                        DepartmentId = c.Int(nullable: false),
                        MajorName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.MajorId)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Semesters",
                c => new
                    {
                        SemesterId = c.Int(nullable: false, identity: true),
                        DateStart = c.DateTime(nullable: false),
                        DateEnd = c.DateTime(nullable: false),
                        FallTransitionDate = c.DateTime(nullable: false),
                        MinimumServiceHours = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SemesterId);
            
            CreateTable(
                "dbo.Leaders",
                c => new
                    {
                        LeaderId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        PositionId = c.Int(nullable: false),
                        AppointedOn = c.DateTime(nullable: false),
                        SemesterId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LeaderId)
                .ForeignKey("dbo.Positions", t => t.PositionId)
                .ForeignKey("dbo.Semesters", t => t.SemesterId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.PositionId)
                .Index(t => t.SemesterId);
            
            CreateTable(
                "dbo.Positions",
                c => new
                    {
                        PositionId = c.Int(nullable: false, identity: true),
                        PositionName = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 100),
                        Type = c.Int(nullable: false),
                        IsExecutive = c.Boolean(nullable: false),
                        IsElected = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        IsDisabled = c.Boolean(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                        CanBeRemoved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PositionId);
            
            CreateTable(
                "dbo.OrganizationsJoined",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        SemesterId = c.Int(nullable: false),
                        OrganizationPositionId = c.Int(),
                    })
                .PrimaryKey(t => new { t.UserId, t.OrganizationId, t.SemesterId })
                .ForeignKey("dbo.OrganizationPositions", t => t.OrganizationPositionId)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .ForeignKey("dbo.Semesters", t => t.SemesterId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.OrganizationId)
                .Index(t => t.SemesterId)
                .Index(t => t.OrganizationPositionId);
            
            CreateTable(
                "dbo.Organizations",
                c => new
                    {
                        OrganizationId = c.Int(nullable: false, identity: true),
                        OrganizationName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.OrganizationId);
            
            CreateTable(
                "dbo.OrganizationPositions",
                c => new
                    {
                        OrganizationPositionId = c.Int(nullable: false, identity: true),
                        OrganizationId = c.Int(nullable: false),
                        PositionName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.OrganizationPositionId)
                .ForeignKey("dbo.Organizations", t => t.OrganizationId)
                .Index(t => t.OrganizationId);
            
            CreateTable(
                "dbo.PledgeClasses",
                c => new
                    {
                        PledgeClassId = c.Int(nullable: false, identity: true),
                        SemesterId = c.Int(nullable: false),
                        PinningDate = c.DateTime(),
                        InitiationDate = c.DateTime(),
                        PledgeClassName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.PledgeClassId)
                .ForeignKey("dbo.Semesters", t => t.SemesterId)
                .Index(t => t.SemesterId);
            
            CreateTable(
                "dbo.IncidentReports",
                c => new
                    {
                        IncidentId = c.Int(nullable: false, identity: true),
                        DateTimeSubmitted = c.DateTime(nullable: false),
                        ReportedBy = c.Int(nullable: false),
                        DateTimeOfIncident = c.DateTime(nullable: false),
                        PolicyBroken = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false, maxLength: 1500),
                        OfficialReport = c.String(maxLength: 1500),
                    })
                .PrimaryKey(t => t.IncidentId)
                .ForeignKey("dbo.Members", t => t.ReportedBy)
                .Index(t => t.ReportedBy);
            
            CreateTable(
                "dbo.LaundrySignup",
                c => new
                    {
                        DateTimeShift = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                        DateTimeSignedUp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DateTimeShift)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.MemberStatus",
                c => new
                    {
                        StatusId = c.Int(nullable: false, identity: true),
                        StatusName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.StatusId);
            
            CreateTable(
                "dbo.PhoneNumbers",
                c => new
                    {
                        PhoneNumberId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        PhoneNumber = c.String(),
                        Type = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.PhoneNumberId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ServiceHours",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        EventId = c.Int(nullable: false),
                        DurationHours = c.Double(nullable: false),
                        DateTimeSubmitted = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.EventId })
                .ForeignKey("dbo.Events", t => t.EventId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        DateTimeOccurred = c.DateTime(nullable: false),
                        EventName = c.String(nullable: false, maxLength: 50),
                        DurationHours = c.Double(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                        SubmitterId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.Members", t => t.SubmitterId, cascadeDelete: true)
                .Index(t => t.SubmitterId);
            
            CreateTable(
                "dbo.SoberSignups",
                c => new
                    {
                        SignupId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(),
                        Type = c.Int(nullable: false),
                        DateOfShift = c.DateTime(nullable: false),
                        DateTimeSignedUp = c.DateTime(),
                    })
                .PrimaryKey(t => t.SignupId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.StudyHours",
                c => new
                    {
                        StudyHourId = c.Int(nullable: false, identity: true),
                        DateTimeStudied = c.DateTime(nullable: false),
                        AssignmentId = c.Int(nullable: false),
                        DurationHours = c.Double(nullable: false),
                        StudyLocation = c.String(nullable: false, maxLength: 50),
                        ApproverId = c.Int(nullable: false),
                        IsProctored = c.Boolean(nullable: false),
                        DateTimeSubmitted = c.DateTime(nullable: false),
                        DateTimeApproved = c.DateTime(),
                    })
                .PrimaryKey(t => t.StudyHourId)
                .ForeignKey("dbo.StudyAssignments", t => t.AssignmentId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.ApproverId)
                .Index(t => new { t.DateTimeStudied, t.AssignmentId }, unique: true, name: "IX_DateTimeAssignment")
                .Index(t => t.ApproverId);
            
            CreateTable(
                "dbo.StudyAssignments",
                c => new
                    {
                        StudyAssignmentId = c.Int(nullable: false, identity: true),
                        AssignedMemberId = c.Int(nullable: false),
                        PeriodId = c.Int(nullable: false),
                        UnproctoredAmount = c.Double(nullable: false),
                        ProctoredAmount = c.Double(nullable: false),
                        AssignedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.StudyAssignmentId)
                .ForeignKey("dbo.Members", t => t.AssignedMemberId, cascadeDelete: true)
                .ForeignKey("dbo.StudyPeriods", t => t.PeriodId, cascadeDelete: true)
                .Index(t => new { t.AssignedMemberId, t.PeriodId, t.UnproctoredAmount, t.ProctoredAmount }, unique: true, name: "IX_AssignedMemberPeriodAmount");
            
            CreateTable(
                "dbo.StudyPeriods",
                c => new
                    {
                        PeriodId = c.Int(nullable: false, identity: true),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.PeriodId)
                .Index(t => new { t.Start, t.End }, unique: true, name: "IX_PeriodDateTime");
            
            CreateTable(
                "dbo.webpages_Membership",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        CreateDate = c.DateTime(),
                        ConfirmationToken = c.String(maxLength: 128),
                        IsConfirmed = c.Boolean(),
                        LastPasswordFailureDate = c.DateTime(),
                        PasswordFailuresSinceLastSuccess = c.Int(nullable: false),
                        Password = c.String(nullable: false, maxLength: 128),
                        PasswordChangedDate = c.DateTime(),
                        PasswordSalt = c.String(nullable: false, maxLength: 128),
                        PasswordVerificationToken = c.String(maxLength: 128),
                        PasswordVerificationTokenExpirationDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.webpages_OAuthMembership",
                c => new
                    {
                        Provider = c.String(nullable: false, maxLength: 30),
                        ProviderUserId = c.String(nullable: false, maxLength: 100),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Provider, t.ProviderUserId })
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Meals",
                c => new
                    {
                        MealId = c.Int(nullable: false, identity: true),
                        MealTitle = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.MealId);
            
            CreateTable(
                "dbo.MealsCooked",
                c => new
                    {
                        ServingId = c.Int(nullable: false, identity: true),
                        DateServed = c.DateTime(nullable: false, storeType: "date"),
                        MealId = c.Int(nullable: false),
                        Lunch = c.Boolean(),
                        Dinner = c.Boolean(),
                    })
                .PrimaryKey(t => t.ServingId)
                .ForeignKey("dbo.Meals", t => t.MealId)
                .Index(t => t.MealId);
            
            CreateTable(
                "dbo.sysdiagrams",
                c => new
                    {
                        diagram_id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 128),
                        principal_id = c.Int(nullable: false),
                        version = c.Int(),
                        definition = c.Binary(),
                    })
                .PrimaryKey(t => t.diagram_id);
            
            CreateTable(
                "dbo.webpages_Roles",
                c => new
                    {
                        RoleId = c.Int(nullable: false, identity: true),
                        RoleName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.RoleId);
            
            CreateTable(
                "dbo.MemberMajors",
                c => new
                    {
                        MajorId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.MajorId, t.UserId })
                .ForeignKey("dbo.Majors", t => t.MajorId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.MajorId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.CommitteeMembers",
                c => new
                    {
                        LeaderId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LeaderId, t.UserId })
                .ForeignKey("dbo.Leaders", t => t.LeaderId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.LeaderId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Members", "webpages_Roles_RoleId", "dbo.webpages_Roles");
            DropForeignKey("dbo.MealsCooked", "MealId", "dbo.Meals");
            DropForeignKey("dbo.webpages_OAuthMembership", "UserId", "dbo.Members");
            DropForeignKey("dbo.webpages_Membership", "UserId", "dbo.Members");
            DropForeignKey("dbo.StudyHours", "ApproverId", "dbo.Members");
            DropForeignKey("dbo.StudyHours", "AssignmentId", "dbo.StudyAssignments");
            DropForeignKey("dbo.StudyAssignments", "PeriodId", "dbo.StudyPeriods");
            DropForeignKey("dbo.StudyAssignments", "AssignedMemberId", "dbo.Members");
            DropForeignKey("dbo.SoberSignups", "UserId", "dbo.Members");
            DropForeignKey("dbo.ServiceHours", "UserId", "dbo.Members");
            DropForeignKey("dbo.Events", "SubmitterId", "dbo.Members");
            DropForeignKey("dbo.ServiceHours", "EventId", "dbo.Events");
            DropForeignKey("dbo.PhoneNumbers", "UserId", "dbo.Members");
            DropForeignKey("dbo.OrganizationsJoined", "UserId", "dbo.Members");
            DropForeignKey("dbo.Members", "StatusId", "dbo.MemberStatus");
            DropForeignKey("dbo.Members", "BigBroId", "dbo.Members");
            DropForeignKey("dbo.Leaders", "UserId", "dbo.Members");
            DropForeignKey("dbo.LaundrySignup", "UserId", "dbo.Members");
            DropForeignKey("dbo.IncidentReports", "ReportedBy", "dbo.Members");
            DropForeignKey("dbo.ClassesTaken", "UserId", "dbo.Members");
            DropForeignKey("dbo.PledgeClasses", "SemesterId", "dbo.Semesters");
            DropForeignKey("dbo.Members", "PledgeClassId", "dbo.PledgeClasses");
            DropForeignKey("dbo.OrganizationsJoined", "SemesterId", "dbo.Semesters");
            DropForeignKey("dbo.OrganizationsJoined", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.OrganizationPositions", "OrganizationId", "dbo.Organizations");
            DropForeignKey("dbo.OrganizationsJoined", "OrganizationPositionId", "dbo.OrganizationPositions");
            DropForeignKey("dbo.Members", "ExpectedGraduationId", "dbo.Semesters");
            DropForeignKey("dbo.Leaders", "SemesterId", "dbo.Semesters");
            DropForeignKey("dbo.Leaders", "PositionId", "dbo.Positions");
            DropForeignKey("dbo.CommitteeMembers", "UserId", "dbo.Members");
            DropForeignKey("dbo.CommitteeMembers", "LeaderId", "dbo.Leaders");
            DropForeignKey("dbo.ClassesTaken", "SemesterId", "dbo.Semesters");
            DropForeignKey("dbo.Majors", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.MemberMajors", "UserId", "dbo.Members");
            DropForeignKey("dbo.MemberMajors", "MajorId", "dbo.Majors");
            DropForeignKey("dbo.Classes", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.ClassesTaken", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.Addresses", "UserId", "dbo.Members");
            DropIndex("dbo.CommitteeMembers", new[] { "UserId" });
            DropIndex("dbo.CommitteeMembers", new[] { "LeaderId" });
            DropIndex("dbo.MemberMajors", new[] { "UserId" });
            DropIndex("dbo.MemberMajors", new[] { "MajorId" });
            DropIndex("dbo.MealsCooked", new[] { "MealId" });
            DropIndex("dbo.webpages_OAuthMembership", new[] { "UserId" });
            DropIndex("dbo.webpages_Membership", new[] { "UserId" });
            DropIndex("dbo.StudyPeriods", "IX_PeriodDateTime");
            DropIndex("dbo.StudyAssignments", "IX_AssignedMemberPeriodAmount");
            DropIndex("dbo.StudyHours", new[] { "ApproverId" });
            DropIndex("dbo.StudyHours", "IX_DateTimeAssignment");
            DropIndex("dbo.SoberSignups", new[] { "UserId" });
            DropIndex("dbo.Events", new[] { "SubmitterId" });
            DropIndex("dbo.ServiceHours", new[] { "EventId" });
            DropIndex("dbo.ServiceHours", new[] { "UserId" });
            DropIndex("dbo.PhoneNumbers", new[] { "UserId" });
            DropIndex("dbo.LaundrySignup", new[] { "UserId" });
            DropIndex("dbo.IncidentReports", new[] { "ReportedBy" });
            DropIndex("dbo.PledgeClasses", new[] { "SemesterId" });
            DropIndex("dbo.OrganizationPositions", new[] { "OrganizationId" });
            DropIndex("dbo.OrganizationsJoined", new[] { "OrganizationPositionId" });
            DropIndex("dbo.OrganizationsJoined", new[] { "SemesterId" });
            DropIndex("dbo.OrganizationsJoined", new[] { "OrganizationId" });
            DropIndex("dbo.OrganizationsJoined", new[] { "UserId" });
            DropIndex("dbo.Leaders", new[] { "SemesterId" });
            DropIndex("dbo.Leaders", new[] { "PositionId" });
            DropIndex("dbo.Leaders", new[] { "UserId" });
            DropIndex("dbo.Majors", new[] { "DepartmentId" });
            DropIndex("dbo.Classes", new[] { "DepartmentId" });
            DropIndex("dbo.ClassesTaken", new[] { "SemesterId" });
            DropIndex("dbo.ClassesTaken", new[] { "ClassId" });
            DropIndex("dbo.ClassesTaken", new[] { "UserId" });
            DropIndex("dbo.Members", new[] { "webpages_Roles_RoleId" });
            DropIndex("dbo.Members", new[] { "BigBroId" });
            DropIndex("dbo.Members", new[] { "ExpectedGraduationId" });
            DropIndex("dbo.Members", new[] { "PledgeClassId" });
            DropIndex("dbo.Members", new[] { "StatusId" });
            DropIndex("dbo.Addresses", new[] { "UserId" });
            DropTable("dbo.CommitteeMembers");
            DropTable("dbo.MemberMajors");
            DropTable("dbo.webpages_Roles");
            DropTable("dbo.sysdiagrams");
            DropTable("dbo.MealsCooked");
            DropTable("dbo.Meals");
            DropTable("dbo.webpages_OAuthMembership");
            DropTable("dbo.webpages_Membership");
            DropTable("dbo.StudyPeriods");
            DropTable("dbo.StudyAssignments");
            DropTable("dbo.StudyHours");
            DropTable("dbo.SoberSignups");
            DropTable("dbo.Events");
            DropTable("dbo.ServiceHours");
            DropTable("dbo.PhoneNumbers");
            DropTable("dbo.MemberStatus");
            DropTable("dbo.LaundrySignup");
            DropTable("dbo.IncidentReports");
            DropTable("dbo.PledgeClasses");
            DropTable("dbo.OrganizationPositions");
            DropTable("dbo.Organizations");
            DropTable("dbo.OrganizationsJoined");
            DropTable("dbo.Positions");
            DropTable("dbo.Leaders");
            DropTable("dbo.Semesters");
            DropTable("dbo.Majors");
            DropTable("dbo.Departments");
            DropTable("dbo.Classes");
            DropTable("dbo.ClassesTaken");
            DropTable("dbo.Members");
            DropTable("dbo.Addresses");
        }
    }
}
