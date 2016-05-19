namespace Dsp.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialChoreEntities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChoreGroupToMembers",
                c => new
                    {
                        ChoreGroupId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChoreGroups", t => t.ChoreGroupId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.ChoreGroupId, t.UserId }, unique: true, name: "IX_ChoreGroupToMember");
            
            CreateTable(
                "dbo.ChoreGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TypeId = c.Int(nullable: false),
                        SemesterId = c.Int(),
                        Name = c.String(nullable: false, maxLength: 50),
                        AvatarPath = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Semesters", t => t.SemesterId)
                .ForeignKey("dbo.ChoreGroupTypes", t => t.TypeId, cascadeDelete: true)
                .Index(t => t.TypeId)
                .Index(t => t.SemesterId);
            
            CreateTable(
                "dbo.ChoreAssignments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChoreId = c.Int(nullable: false),
                        PeriodId = c.Int(nullable: false),
                        GroupId = c.Int(nullable: false),
                        OpensOnCst = c.DateTime(nullable: false),
                        DueOnCst = c.DateTime(nullable: false),
                        GroupCompleted = c.Boolean(nullable: false),
                        GroupCompletionTimeCst = c.DateTime(nullable: false),
                        GroupSignerId = c.Int(),
                        EnforcerVerified = c.Boolean(),
                        EnforcerVerificationTimeCst = c.DateTime(nullable: false),
                        EnforcerId = c.Int(),
                        EnforcerFeedback = c.String(),
                        IsCancelled = c.Boolean(nullable: false),
                        EnforcementChoreId = c.Int(),
                        CreatedOnCst = c.DateTime(nullable: false),
                        CancelledOnCst = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Chores", t => t.ChoreId, cascadeDelete: true)
                .ForeignKey("dbo.ChoreAssignments", t => t.EnforcementChoreId)
                .ForeignKey("dbo.ChorePeriods", t => t.PeriodId, cascadeDelete: true)
                .ForeignKey("dbo.ChoreGroups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.EnforcerId)
                .ForeignKey("dbo.Members", t => t.GroupSignerId)
                .Index(t => t.ChoreId)
                .Index(t => t.PeriodId)
                .Index(t => t.GroupId)
                .Index(t => t.GroupSignerId)
                .Index(t => t.EnforcerId)
                .Index(t => t.EnforcementChoreId);
            
            CreateTable(
                "dbo.Chores",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TypeId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        Url = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChoreTypes", t => t.TypeId, cascadeDelete: true)
                .Index(t => t.TypeId);
            
            CreateTable(
                "dbo.ChoreTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChorePeriods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BeginsOnCst = c.DateTime(nullable: false),
                        EndsOnCst = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChoreGroupTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChoreAssignments", "GroupSignerId", "dbo.Members");
            DropForeignKey("dbo.ChoreAssignments", "EnforcerId", "dbo.Members");
            DropForeignKey("dbo.ChoreGroupToMembers", "UserId", "dbo.Members");
            DropForeignKey("dbo.ChoreGroups", "TypeId", "dbo.ChoreGroupTypes");
            DropForeignKey("dbo.ChoreGroups", "SemesterId", "dbo.Semesters");
            DropForeignKey("dbo.ChoreGroupToMembers", "ChoreGroupId", "dbo.ChoreGroups");
            DropForeignKey("dbo.ChoreAssignments", "GroupId", "dbo.ChoreGroups");
            DropForeignKey("dbo.ChoreAssignments", "PeriodId", "dbo.ChorePeriods");
            DropForeignKey("dbo.ChoreAssignments", "EnforcementChoreId", "dbo.ChoreAssignments");
            DropForeignKey("dbo.Chores", "TypeId", "dbo.ChoreTypes");
            DropForeignKey("dbo.ChoreAssignments", "ChoreId", "dbo.Chores");
            DropIndex("dbo.Chores", new[] { "TypeId" });
            DropIndex("dbo.ChoreAssignments", new[] { "EnforcementChoreId" });
            DropIndex("dbo.ChoreAssignments", new[] { "EnforcerId" });
            DropIndex("dbo.ChoreAssignments", new[] { "GroupSignerId" });
            DropIndex("dbo.ChoreAssignments", new[] { "GroupId" });
            DropIndex("dbo.ChoreAssignments", new[] { "PeriodId" });
            DropIndex("dbo.ChoreAssignments", new[] { "ChoreId" });
            DropIndex("dbo.ChoreGroups", new[] { "SemesterId" });
            DropIndex("dbo.ChoreGroups", new[] { "TypeId" });
            DropIndex("dbo.ChoreGroupToMembers", "IX_ChoreGroupToMember");
            DropTable("dbo.ChoreGroupTypes");
            DropTable("dbo.ChorePeriods");
            DropTable("dbo.ChoreTypes");
            DropTable("dbo.Chores");
            DropTable("dbo.ChoreAssignments");
            DropTable("dbo.ChoreGroups");
            DropTable("dbo.ChoreGroupToMembers");
        }
    }
}
