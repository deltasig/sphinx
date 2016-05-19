namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StudyHours : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StudyAssignments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PeriodId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                        AmountOfHours = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StudyPeriods", t => t.PeriodId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.MemberId, cascadeDelete: true)
                .Index(t => t.PeriodId)
                .Index(t => t.MemberId);
            
            CreateTable(
                "dbo.StudyPeriods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BeginsOn = c.DateTime(nullable: false),
                        EndsOn = c.DateTime(nullable: false),
                        FineAmount = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StudyHours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssignmentId = c.Int(),
                        SessionId = c.Int(),
                        MemberId = c.Int(nullable: false),
                        SignedInOn = c.DateTime(nullable: false),
                        SignedOutOn = c.DateTime(nullable: false),
                        DurationMinutes = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StudySessions", t => t.SessionId)
                .ForeignKey("dbo.StudyAssignments", t => t.AssignmentId)
                .ForeignKey("dbo.Members", t => t.MemberId, cascadeDelete: true)
                .Index(t => t.AssignmentId)
                .Index(t => t.SessionId)
                .Index(t => t.MemberId);
            
            CreateTable(
                "dbo.StudySessions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Location = c.String(),
                        BeginsOn = c.DateTime(nullable: false),
                        EndsOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StudyProctors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SessionId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StudySessions", t => t.SessionId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.MemberId, cascadeDelete: true)
                .Index(t => t.SessionId)
                .Index(t => t.MemberId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StudyProctors", "MemberId", "dbo.Members");
            DropForeignKey("dbo.StudyHours", "MemberId", "dbo.Members");
            DropForeignKey("dbo.StudyAssignments", "MemberId", "dbo.Members");
            DropForeignKey("dbo.StudyHours", "AssignmentId", "dbo.StudyAssignments");
            DropForeignKey("dbo.StudyHours", "SessionId", "dbo.StudySessions");
            DropForeignKey("dbo.StudyProctors", "SessionId", "dbo.StudySessions");
            DropForeignKey("dbo.StudyAssignments", "PeriodId", "dbo.StudyPeriods");
            DropIndex("dbo.StudyProctors", new[] { "MemberId" });
            DropIndex("dbo.StudyProctors", new[] { "SessionId" });
            DropIndex("dbo.StudyHours", new[] { "MemberId" });
            DropIndex("dbo.StudyHours", new[] { "SessionId" });
            DropIndex("dbo.StudyHours", new[] { "AssignmentId" });
            DropIndex("dbo.StudyAssignments", new[] { "MemberId" });
            DropIndex("dbo.StudyAssignments", new[] { "PeriodId" });
            DropTable("dbo.StudyProctors");
            DropTable("dbo.StudySessions");
            DropTable("dbo.StudyHours");
            DropTable("dbo.StudyPeriods");
            DropTable("dbo.StudyAssignments");
        }
    }
}
