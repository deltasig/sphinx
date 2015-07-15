namespace Dsp.Migrations
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
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Members",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 50),
                        Pin = c.Int(),
                        Room = c.Int(),
                        StatusId = c.Int(nullable: false),
                        PledgeClassId = c.Int(),
                        ExpectedGraduationId = c.Int(),
                        BigBroId = c.Int(),
                        ShirtSize = c.String(),
                        CreatedOn = c.DateTime(),
                        LastUpdatedOn = c.DateTime(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Semesters", t => t.ExpectedGraduationId)
                .ForeignKey("dbo.PledgeClasses", t => t.PledgeClassId)
                .ForeignKey("dbo.Members", t => t.BigBroId)
                .ForeignKey("dbo.MemberStatuses", t => t.StatusId, cascadeDelete: true)
                .Index(t => t.StatusId)
                .Index(t => t.PledgeClassId)
                .Index(t => t.ExpectedGraduationId)
                .Index(t => t.BigBroId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.MemberClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ClassesTaken",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        ClassId = c.Int(nullable: false),
                        SemesterId = c.Int(nullable: false),
                        ClassTakenId = c.Int(nullable: false, identity: true),
                        MidtermGrade = c.String(maxLength: 1),
                        FinalGrade = c.String(maxLength: 1),
                        Dropped = c.Boolean(),
                        IsSummerClass = c.Boolean(nullable: false),
                        CreatedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.ClassTakenId)
                .ForeignKey("dbo.Classes", t => t.ClassId, cascadeDelete: true)
                .ForeignKey("dbo.Semesters", t => t.SemesterId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.ClassId, t.SemesterId }, unique: true, name: "IX_ClassTaken");
            
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
                .ForeignKey("dbo.Departments", t => t.DepartmentId, cascadeDelete: true)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.ClassFiles",
                c => new
                    {
                        ClassFileId = c.Int(nullable: false, identity: true),
                        ClassId = c.Int(nullable: false),
                        UserId = c.Int(),
                        UploadedOn = c.DateTime(nullable: false),
                        LastAccessedOn = c.DateTime(),
                        DownloadCount = c.Int(nullable: false),
                        AwsCode = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.ClassFileId)
                .ForeignKey("dbo.Classes", t => t.ClassId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.ClassId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ClassFileVotes",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        ClassFileId = c.Int(nullable: false),
                        ClassFileVoteId = c.Int(nullable: false, identity: true),
                        IsUpvote = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ClassFileVoteId)
                .ForeignKey("dbo.ClassFiles", t => t.ClassFileId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.ClassFileId }, unique: true, name: "IX_ClassFileVote");
            
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        DepartmentId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
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
                .ForeignKey("dbo.Departments", t => t.DepartmentId, cascadeDelete: true)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.MajorToMembers",
                c => new
                    {
                        MajorId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        DegreeLevel = c.Int(nullable: false),
                        MajorToMemberId = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.MajorToMemberId)
                .ForeignKey("dbo.Majors", t => t.MajorId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.MajorId, t.UserId, t.DegreeLevel }, unique: true, name: "IX_MajorToMember");
            
            CreateTable(
                "dbo.Semesters",
                c => new
                    {
                        SemesterId = c.Int(nullable: false, identity: true),
                        DateStart = c.DateTime(nullable: false),
                        DateEnd = c.DateTime(nullable: false),
                        TransitionDate = c.DateTime(nullable: false),
                        MinimumServiceHours = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SemesterId);
            
            CreateTable(
                "dbo.Leaders",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        LeaderId = c.Int(nullable: false),
                        SemesterId = c.Int(nullable: false),
                        AppointedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.LeaderId })
                .ForeignKey("dbo.Positions", t => t.LeaderId, cascadeDelete: true)
                .ForeignKey("dbo.Semesters", t => t.SemesterId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.LeaderId)
                .Index(t => t.SemesterId);
            
            CreateTable(
                "dbo.Positions",
                c => new
                    {
                        PositionId = c.Int(nullable: false, identity: true),
                        Description = c.String(maxLength: 100),
                        Inquiries = c.String(maxLength: 50),
                        Type = c.Int(nullable: false),
                        IsExecutive = c.Boolean(nullable: false),
                        IsElected = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        IsDisabled = c.Boolean(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                        CanBeRemoved = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.PositionId)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
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
                .ForeignKey("dbo.Semesters", t => t.SemesterId, cascadeDelete: true)
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
                .ForeignKey("dbo.Members", t => t.ReportedBy, cascadeDelete: true)
                .Index(t => t.ReportedBy);
            
            CreateTable(
                "dbo.LaundrySignups",
                c => new
                    {
                        DateTimeShift = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                        DateTimeSignedUp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DateTimeShift)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.MemberLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.MealPlates",
                c => new
                    {
                        MealPlateId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        PlateDateTime = c.DateTime(nullable: false),
                        SignedUpOn = c.DateTime(nullable: false),
                        Type = c.String(maxLength: 25),
                    })
                .PrimaryKey(t => t.MealPlateId)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.MealVotes",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        MealItemId = c.Int(nullable: false),
                        MealVoteId = c.Int(nullable: false, identity: true),
                        IsUpvote = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.MealVoteId)
                .ForeignKey("dbo.MealItems", t => t.MealItemId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.MealItemId }, unique: true, name: "IX_MealVote");
            
            CreateTable(
                "dbo.MealItems",
                c => new
                    {
                        MealItemId = c.Int(nullable: false, identity: true),
                        MealItemTypeId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 150),
                        IsGlutenFree = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.MealItemId)
                .ForeignKey("dbo.MealItemTypes", t => t.MealItemTypeId, cascadeDelete: true)
                .Index(t => t.MealItemTypeId);
            
            CreateTable(
                "dbo.MealItemTypes",
                c => new
                    {
                        MealItemTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.MealItemTypeId);
            
            CreateTable(
                "dbo.MealToItems",
                c => new
                    {
                        MealId = c.Int(nullable: false),
                        MealItemId = c.Int(nullable: false),
                        MealToItemId = c.Int(nullable: false, identity: true),
                        DisplayOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MealToItemId)
                .ForeignKey("dbo.Meals", t => t.MealId, cascadeDelete: true)
                .ForeignKey("dbo.MealItems", t => t.MealItemId, cascadeDelete: true)
                .Index(t => new { t.MealId, t.MealItemId }, unique: true, name: "IX_MealToItem");
            
            CreateTable(
                "dbo.Meals",
                c => new
                    {
                        MealId = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.MealId);
            
            CreateTable(
                "dbo.MealToPeriods",
                c => new
                    {
                        MealToPeriodId = c.Int(nullable: false, identity: true),
                        MealPeriodId = c.Int(nullable: false),
                        MealId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false, storeType: "date"),
                    })
                .PrimaryKey(t => t.MealToPeriodId)
                .ForeignKey("dbo.MealPeriods", t => t.MealPeriodId, cascadeDelete: true)
                .ForeignKey("dbo.Meals", t => t.MealId, cascadeDelete: true)
                .Index(t => t.MealPeriodId)
                .Index(t => t.MealId);
            
            CreateTable(
                "dbo.MealPeriods",
                c => new
                    {
                        MealPeriodId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MealPeriodId);
            
            CreateTable(
                "dbo.MemberStatuses",
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
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.RoomToMembers",
                c => new
                    {
                        RoomToMemberId = c.Int(nullable: false, identity: true),
                        RoomId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        MovedIn = c.DateTime(nullable: false),
                        MovedOut = c.DateTime(),
                    })
                .PrimaryKey(t => t.RoomToMemberId)
                .ForeignKey("dbo.Rooms", t => t.RoomId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.RoomId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        RoomId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        MaxCapacity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RoomId);
            
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
                .ForeignKey("dbo.Events", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        SubmitterId = c.Int(),
                        IsApproved = c.Boolean(nullable: false),
                        DateTimeOccurred = c.DateTime(nullable: false),
                        EventName = c.String(nullable: false, maxLength: 50),
                        DurationHours = c.Double(nullable: false),
                        CreatedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.Members", t => t.SubmitterId)
                .Index(t => t.SubmitterId);
            
            CreateTable(
                "dbo.SoberSignups",
                c => new
                    {
                        SignupId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(),
                        SoberTypeId = c.Int(nullable: false),
                        Description = c.String(maxLength: 100),
                        DateOfShift = c.DateTime(nullable: false),
                        DateTimeSignedUp = c.DateTime(),
                        CreatedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.SignupId)
                .ForeignKey("dbo.SoberTypes", t => t.SoberTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.SoberTypeId);
            
            CreateTable(
                "dbo.SoberTypes",
                c => new
                    {
                        SoberTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 3000),
                    })
                .PrimaryKey(t => t.SoberTypeId);
            
            CreateTable(
                "dbo.WorkOrderComments",
                c => new
                    {
                        WorkOrderCommentId = c.Int(nullable: false, identity: true),
                        WorkOrderId = c.Int(nullable: false),
                        UserId = c.Int(),
                        SubmittedOn = c.DateTime(nullable: false),
                        Text = c.String(nullable: false, maxLength: 3000),
                    })
                .PrimaryKey(t => t.WorkOrderCommentId)
                .ForeignKey("dbo.WorkOrders", t => t.WorkOrderId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.WorkOrderId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.WorkOrders",
                c => new
                    {
                        WorkOrderId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false, maxLength: 3000),
                        Result = c.String(maxLength: 3000),
                    })
                .PrimaryKey(t => t.WorkOrderId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.WorkOrderPriorityChanges",
                c => new
                    {
                        WorkOrderPriorityChangeId = c.Int(nullable: false, identity: true),
                        WorkOrderId = c.Int(nullable: false),
                        WorkOrderPriorityId = c.Int(nullable: false),
                        UserId = c.Int(),
                        ChangedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.WorkOrderPriorityChangeId)
                .ForeignKey("dbo.WorkOrderPriorities", t => t.WorkOrderPriorityId, cascadeDelete: true)
                .ForeignKey("dbo.WorkOrders", t => t.WorkOrderId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.WorkOrderId)
                .Index(t => t.WorkOrderPriorityId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.WorkOrderPriorities",
                c => new
                    {
                        WorkOrderPriorityId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.WorkOrderPriorityId);
            
            CreateTable(
                "dbo.WorkOrderStatusChanges",
                c => new
                    {
                        WorkOrderStatusChangeId = c.Int(nullable: false, identity: true),
                        WorkOrderId = c.Int(nullable: false),
                        WorkOrderStatusId = c.Int(nullable: false),
                        UserId = c.Int(),
                        ChangedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.WorkOrderStatusChangeId)
                .ForeignKey("dbo.WorkOrderStatuses", t => t.WorkOrderStatusId, cascadeDelete: true)
                .ForeignKey("dbo.WorkOrders", t => t.WorkOrderId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.WorkOrderId)
                .Index(t => t.WorkOrderStatusId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.WorkOrderStatuses",
                c => new
                    {
                        WorkOrderStatusId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.WorkOrderStatusId);
            
            CreateTable(
                "dbo.Emails",
                c => new
                    {
                        EmailId = c.Int(nullable: false, identity: true),
                        EmailTypeId = c.Int(nullable: false),
                        SentOn = c.DateTime(nullable: false),
                        Destination = c.String(),
                        Body = c.String(),
                    })
                .PrimaryKey(t => t.EmailId)
                .ForeignKey("dbo.EmailTypes", t => t.EmailTypeId, cascadeDelete: true)
                .Index(t => t.EmailTypeId);
            
            CreateTable(
                "dbo.EmailTypes",
                c => new
                    {
                        EmailTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        Destination = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.EmailTypeId);
            
            CreateTable(
                "dbo.ScholarshipAnswers",
                c => new
                    {
                        ScholarshipAnswerId = c.Int(nullable: false, identity: true),
                        ScholarshipSubmissionId = c.Guid(nullable: false),
                        ScholarshipQuestionId = c.Int(nullable: false),
                        AnswerText = c.String(maxLength: 3000),
                    })
                .PrimaryKey(t => t.ScholarshipAnswerId)
                .ForeignKey("dbo.ScholarshipQuestions", t => t.ScholarshipQuestionId, cascadeDelete: true)
                .ForeignKey("dbo.ScholarshipSubmissions", t => t.ScholarshipSubmissionId, cascadeDelete: true)
                .Index(t => t.ScholarshipSubmissionId)
                .Index(t => t.ScholarshipQuestionId);
            
            CreateTable(
                "dbo.ScholarshipQuestions",
                c => new
                    {
                        ScholarshipQuestionId = c.Int(nullable: false, identity: true),
                        Prompt = c.String(nullable: false, maxLength: 500),
                        AnswerMinimumLength = c.Int(nullable: false),
                        AnswerMaximumLength = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ScholarshipQuestionId);
            
            CreateTable(
                "dbo.ScholarshipAppQuestions",
                c => new
                    {
                        ScholarshipAppQuestionId = c.Int(nullable: false, identity: true),
                        ScholarshipAppId = c.Int(nullable: false),
                        ScholarshipQuestionId = c.Int(nullable: false),
                        FormOrder = c.Int(nullable: false),
                        IsOptional = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ScholarshipAppQuestionId)
                .ForeignKey("dbo.ScholarshipApps", t => t.ScholarshipAppId, cascadeDelete: true)
                .ForeignKey("dbo.ScholarshipQuestions", t => t.ScholarshipQuestionId, cascadeDelete: true)
                .Index(t => t.ScholarshipAppId)
                .Index(t => t.ScholarshipQuestionId);
            
            CreateTable(
                "dbo.ScholarshipApps",
                c => new
                    {
                        ScholarshipAppId = c.Int(nullable: false, identity: true),
                        ScholarshipTypeId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 100),
                        AdditionalText = c.String(nullable: false, maxLength: 3000),
                        OpensOn = c.DateTime(nullable: false),
                        ClosesOn = c.DateTime(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ScholarshipAppId)
                .ForeignKey("dbo.ScholarshipTypes", t => t.ScholarshipTypeId, cascadeDelete: true)
                .Index(t => t.ScholarshipTypeId);
            
            CreateTable(
                "dbo.ScholarshipSubmissions",
                c => new
                    {
                        ScholarshipSubmissionId = c.Guid(nullable: false, identity: true),
                        ScholarshipAppId = c.Int(nullable: false),
                        IsWinner = c.Boolean(nullable: false),
                        SubmittedOn = c.DateTime(nullable: false),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        StudentNumber = c.String(nullable: false, maxLength: 15),
                        PhoneNumber = c.String(nullable: false, maxLength: 15),
                        Email = c.String(nullable: false, maxLength: 50),
                        Address1 = c.String(nullable: false, maxLength: 100),
                        Address2 = c.String(maxLength: 100),
                        City = c.String(nullable: false, maxLength: 50),
                        State = c.String(maxLength: 2),
                        PostalCode = c.Int(nullable: false),
                        Country = c.String(nullable: false, maxLength: 50),
                        HighSchool = c.String(nullable: false, maxLength: 100),
                        ActSatScore = c.Int(nullable: false),
                        Gpa = c.Double(nullable: false),
                        HearAboutScholarship = c.String(nullable: false, maxLength: 100),
                        CommitteeResponse = c.String(),
                        CommitteeRespondedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.ScholarshipSubmissionId)
                .ForeignKey("dbo.ScholarshipApps", t => t.ScholarshipAppId, cascadeDelete: true)
                .Index(t => t.ScholarshipAppId);
            
            CreateTable(
                "dbo.ScholarshipTypes",
                c => new
                    {
                        ScholarshipTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.ScholarshipTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ScholarshipAppQuestions", "ScholarshipQuestionId", "dbo.ScholarshipQuestions");
            DropForeignKey("dbo.ScholarshipApps", "ScholarshipTypeId", "dbo.ScholarshipTypes");
            DropForeignKey("dbo.ScholarshipSubmissions", "ScholarshipAppId", "dbo.ScholarshipApps");
            DropForeignKey("dbo.ScholarshipAnswers", "ScholarshipSubmissionId", "dbo.ScholarshipSubmissions");
            DropForeignKey("dbo.ScholarshipAppQuestions", "ScholarshipAppId", "dbo.ScholarshipApps");
            DropForeignKey("dbo.ScholarshipAnswers", "ScholarshipQuestionId", "dbo.ScholarshipQuestions");
            DropForeignKey("dbo.Emails", "EmailTypeId", "dbo.EmailTypes");
            DropForeignKey("dbo.WorkOrderStatusChanges", "UserId", "dbo.Members");
            DropForeignKey("dbo.WorkOrders", "UserId", "dbo.Members");
            DropForeignKey("dbo.WorkOrderPriorityChanges", "UserId", "dbo.Members");
            DropForeignKey("dbo.WorkOrderComments", "UserId", "dbo.Members");
            DropForeignKey("dbo.WorkOrderStatusChanges", "WorkOrderId", "dbo.WorkOrders");
            DropForeignKey("dbo.WorkOrderStatusChanges", "WorkOrderStatusId", "dbo.WorkOrderStatuses");
            DropForeignKey("dbo.WorkOrderPriorityChanges", "WorkOrderId", "dbo.WorkOrders");
            DropForeignKey("dbo.WorkOrderPriorityChanges", "WorkOrderPriorityId", "dbo.WorkOrderPriorities");
            DropForeignKey("dbo.WorkOrderComments", "WorkOrderId", "dbo.WorkOrders");
            DropForeignKey("dbo.Events", "SubmitterId", "dbo.Members");
            DropForeignKey("dbo.SoberSignups", "UserId", "dbo.Members");
            DropForeignKey("dbo.SoberSignups", "SoberTypeId", "dbo.SoberTypes");
            DropForeignKey("dbo.ServiceHours", "UserId", "dbo.Members");
            DropForeignKey("dbo.ServiceHours", "EventId", "dbo.Events");
            DropForeignKey("dbo.RoomToMembers", "UserId", "dbo.Members");
            DropForeignKey("dbo.RoomToMembers", "RoomId", "dbo.Rooms");
            DropForeignKey("dbo.PhoneNumbers", "UserId", "dbo.Members");
            DropForeignKey("dbo.Members", "StatusId", "dbo.MemberStatuses");
            DropForeignKey("dbo.MealVotes", "UserId", "dbo.Members");
            DropForeignKey("dbo.MealVotes", "MealItemId", "dbo.MealItems");
            DropForeignKey("dbo.MealToItems", "MealItemId", "dbo.MealItems");
            DropForeignKey("dbo.MealToPeriods", "MealId", "dbo.Meals");
            DropForeignKey("dbo.MealToPeriods", "MealPeriodId", "dbo.MealPeriods");
            DropForeignKey("dbo.MealToItems", "MealId", "dbo.Meals");
            DropForeignKey("dbo.MealItems", "MealItemTypeId", "dbo.MealItemTypes");
            DropForeignKey("dbo.MealPlates", "UserId", "dbo.Members");
            DropForeignKey("dbo.MajorToMembers", "UserId", "dbo.Members");
            DropForeignKey("dbo.MemberLogins", "UserId", "dbo.Members");
            DropForeignKey("dbo.Members", "BigBroId", "dbo.Members");
            DropForeignKey("dbo.Leaders", "UserId", "dbo.Members");
            DropForeignKey("dbo.LaundrySignups", "UserId", "dbo.Members");
            DropForeignKey("dbo.IncidentReports", "ReportedBy", "dbo.Members");
            DropForeignKey("dbo.ClassFileVotes", "UserId", "dbo.Members");
            DropForeignKey("dbo.ClassFiles", "UserId", "dbo.Members");
            DropForeignKey("dbo.ClassesTaken", "UserId", "dbo.Members");
            DropForeignKey("dbo.PledgeClasses", "SemesterId", "dbo.Semesters");
            DropForeignKey("dbo.Members", "PledgeClassId", "dbo.PledgeClasses");
            DropForeignKey("dbo.Leaders", "SemesterId", "dbo.Semesters");
            DropForeignKey("dbo.Leaders", "LeaderId", "dbo.Positions");
            DropForeignKey("dbo.Members", "ExpectedGraduationId", "dbo.Semesters");
            DropForeignKey("dbo.ClassesTaken", "SemesterId", "dbo.Semesters");
            DropForeignKey("dbo.Majors", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.MajorToMembers", "MajorId", "dbo.Majors");
            DropForeignKey("dbo.Classes", "DepartmentId", "dbo.Departments");
            DropForeignKey("dbo.ClassFiles", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.ClassFileVotes", "ClassFileId", "dbo.ClassFiles");
            DropForeignKey("dbo.ClassesTaken", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.MemberClaims", "UserId", "dbo.Members");
            DropForeignKey("dbo.Addresses", "UserId", "dbo.Members");
            DropIndex("dbo.ScholarshipSubmissions", new[] { "ScholarshipAppId" });
            DropIndex("dbo.ScholarshipApps", new[] { "ScholarshipTypeId" });
            DropIndex("dbo.ScholarshipAppQuestions", new[] { "ScholarshipQuestionId" });
            DropIndex("dbo.ScholarshipAppQuestions", new[] { "ScholarshipAppId" });
            DropIndex("dbo.ScholarshipAnswers", new[] { "ScholarshipQuestionId" });
            DropIndex("dbo.ScholarshipAnswers", new[] { "ScholarshipSubmissionId" });
            DropIndex("dbo.Emails", new[] { "EmailTypeId" });
            DropIndex("dbo.WorkOrderStatusChanges", new[] { "UserId" });
            DropIndex("dbo.WorkOrderStatusChanges", new[] { "WorkOrderStatusId" });
            DropIndex("dbo.WorkOrderStatusChanges", new[] { "WorkOrderId" });
            DropIndex("dbo.WorkOrderPriorityChanges", new[] { "UserId" });
            DropIndex("dbo.WorkOrderPriorityChanges", new[] { "WorkOrderPriorityId" });
            DropIndex("dbo.WorkOrderPriorityChanges", new[] { "WorkOrderId" });
            DropIndex("dbo.WorkOrders", new[] { "UserId" });
            DropIndex("dbo.WorkOrderComments", new[] { "UserId" });
            DropIndex("dbo.WorkOrderComments", new[] { "WorkOrderId" });
            DropIndex("dbo.SoberSignups", new[] { "SoberTypeId" });
            DropIndex("dbo.SoberSignups", new[] { "UserId" });
            DropIndex("dbo.Events", new[] { "SubmitterId" });
            DropIndex("dbo.ServiceHours", new[] { "EventId" });
            DropIndex("dbo.ServiceHours", new[] { "UserId" });
            DropIndex("dbo.RoomToMembers", new[] { "UserId" });
            DropIndex("dbo.RoomToMembers", new[] { "RoomId" });
            DropIndex("dbo.PhoneNumbers", new[] { "UserId" });
            DropIndex("dbo.MealToPeriods", new[] { "MealId" });
            DropIndex("dbo.MealToPeriods", new[] { "MealPeriodId" });
            DropIndex("dbo.MealToItems", "IX_MealToItem");
            DropIndex("dbo.MealItems", new[] { "MealItemTypeId" });
            DropIndex("dbo.MealVotes", "IX_MealVote");
            DropIndex("dbo.MealPlates", new[] { "UserId" });
            DropIndex("dbo.MemberLogins", new[] { "UserId" });
            DropIndex("dbo.LaundrySignups", new[] { "UserId" });
            DropIndex("dbo.IncidentReports", new[] { "ReportedBy" });
            DropIndex("dbo.PledgeClasses", new[] { "SemesterId" });
            DropIndex("dbo.Positions", "RoleNameIndex");
            DropIndex("dbo.Leaders", new[] { "SemesterId" });
            DropIndex("dbo.Leaders", new[] { "LeaderId" });
            DropIndex("dbo.Leaders", new[] { "UserId" });
            DropIndex("dbo.MajorToMembers", "IX_MajorToMember");
            DropIndex("dbo.Majors", new[] { "DepartmentId" });
            DropIndex("dbo.ClassFileVotes", "IX_ClassFileVote");
            DropIndex("dbo.ClassFiles", new[] { "UserId" });
            DropIndex("dbo.ClassFiles", new[] { "ClassId" });
            DropIndex("dbo.Classes", new[] { "DepartmentId" });
            DropIndex("dbo.ClassesTaken", "IX_ClassTaken");
            DropIndex("dbo.MemberClaims", new[] { "UserId" });
            DropIndex("dbo.Members", "UserNameIndex");
            DropIndex("dbo.Members", new[] { "BigBroId" });
            DropIndex("dbo.Members", new[] { "ExpectedGraduationId" });
            DropIndex("dbo.Members", new[] { "PledgeClassId" });
            DropIndex("dbo.Members", new[] { "StatusId" });
            DropIndex("dbo.Addresses", new[] { "UserId" });
            DropTable("dbo.ScholarshipTypes");
            DropTable("dbo.ScholarshipSubmissions");
            DropTable("dbo.ScholarshipApps");
            DropTable("dbo.ScholarshipAppQuestions");
            DropTable("dbo.ScholarshipQuestions");
            DropTable("dbo.ScholarshipAnswers");
            DropTable("dbo.EmailTypes");
            DropTable("dbo.Emails");
            DropTable("dbo.WorkOrderStatuses");
            DropTable("dbo.WorkOrderStatusChanges");
            DropTable("dbo.WorkOrderPriorities");
            DropTable("dbo.WorkOrderPriorityChanges");
            DropTable("dbo.WorkOrders");
            DropTable("dbo.WorkOrderComments");
            DropTable("dbo.SoberTypes");
            DropTable("dbo.SoberSignups");
            DropTable("dbo.Events");
            DropTable("dbo.ServiceHours");
            DropTable("dbo.Rooms");
            DropTable("dbo.RoomToMembers");
            DropTable("dbo.PhoneNumbers");
            DropTable("dbo.MemberStatuses");
            DropTable("dbo.MealPeriods");
            DropTable("dbo.MealToPeriods");
            DropTable("dbo.Meals");
            DropTable("dbo.MealToItems");
            DropTable("dbo.MealItemTypes");
            DropTable("dbo.MealItems");
            DropTable("dbo.MealVotes");
            DropTable("dbo.MealPlates");
            DropTable("dbo.MemberLogins");
            DropTable("dbo.LaundrySignups");
            DropTable("dbo.IncidentReports");
            DropTable("dbo.PledgeClasses");
            DropTable("dbo.Positions");
            DropTable("dbo.Leaders");
            DropTable("dbo.Semesters");
            DropTable("dbo.MajorToMembers");
            DropTable("dbo.Majors");
            DropTable("dbo.Departments");
            DropTable("dbo.ClassFileVotes");
            DropTable("dbo.ClassFiles");
            DropTable("dbo.Classes");
            DropTable("dbo.ClassesTaken");
            DropTable("dbo.MemberClaims");
            DropTable("dbo.Members");
            DropTable("dbo.Addresses");
        }
    }
}
