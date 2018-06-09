namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedStudyEntities : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.StudyAssignments", "PeriodId", "dbo.StudyPeriods");
            DropForeignKey("dbo.StudyProctors", "SessionId", "dbo.StudySessions");
            DropForeignKey("dbo.StudyHours", "SessionId", "dbo.StudySessions");
            DropForeignKey("dbo.StudyHours", "AssignmentId", "dbo.StudyAssignments");
            DropForeignKey("dbo.StudyAssignments", "MemberId", "dbo.Members");
            DropForeignKey("dbo.StudyHours", "MemberId", "dbo.Members");
            DropForeignKey("dbo.StudyProctors", "MemberId", "dbo.Members");
            DropIndex("dbo.StudyAssignments", new[] { "PeriodId" });
            DropIndex("dbo.StudyAssignments", new[] { "MemberId" });
            DropIndex("dbo.StudyHours", new[] { "AssignmentId" });
            DropIndex("dbo.StudyHours", new[] { "SessionId" });
            DropIndex("dbo.StudyHours", new[] { "MemberId" });
            DropIndex("dbo.StudyProctors", new[] { "SessionId" });
            DropIndex("dbo.StudyProctors", new[] { "MemberId" });
            DropTable("dbo.StudyAssignments");
            DropTable("dbo.StudyPeriods");
            DropTable("dbo.StudyHours");
            DropTable("dbo.StudySessions");
            DropTable("dbo.StudyProctors");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.StudyProctors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SessionId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StudySessions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Location = c.String(nullable: false),
                        BeginsOn = c.DateTime(nullable: false),
                        EndsOn = c.DateTime(nullable: false),
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
                        SignedOutOn = c.DateTime(),
                        DurationMinutes = c.Double(),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.StudyAssignments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PeriodId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                        AmountOfHours = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.StudyProctors", "MemberId");
            CreateIndex("dbo.StudyProctors", "SessionId");
            CreateIndex("dbo.StudyHours", "MemberId");
            CreateIndex("dbo.StudyHours", "SessionId");
            CreateIndex("dbo.StudyHours", "AssignmentId");
            CreateIndex("dbo.StudyAssignments", "MemberId");
            CreateIndex("dbo.StudyAssignments", "PeriodId");
            AddForeignKey("dbo.StudyProctors", "MemberId", "dbo.Members", "UserId", cascadeDelete: true);
            AddForeignKey("dbo.StudyHours", "MemberId", "dbo.Members", "UserId", cascadeDelete: true);
            AddForeignKey("dbo.StudyAssignments", "MemberId", "dbo.Members", "UserId", cascadeDelete: true);
            AddForeignKey("dbo.StudyHours", "AssignmentId", "dbo.StudyAssignments", "Id");
            AddForeignKey("dbo.StudyHours", "SessionId", "dbo.StudySessions", "Id");
            AddForeignKey("dbo.StudyProctors", "SessionId", "dbo.StudySessions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StudyAssignments", "PeriodId", "dbo.StudyPeriods", "Id", cascadeDelete: true);
        }
    }
}
