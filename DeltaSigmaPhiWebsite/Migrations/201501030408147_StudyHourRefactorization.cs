namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StudyHourRefactorization : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.StudyHours", "SubmittedBy", "dbo.Members");
            DropIndex("dbo.StudyHours", new[] { "SubmittedBy" });
            DropIndex("dbo.StudyHours", new[] { "ApproverId" });
            DropPrimaryKey("dbo.StudyHours");
            CreateTable(
                "dbo.MemberStudyHourAssignments",
                c => new
                    {
                        MemberStudyHourAssignmentId = c.Int(nullable: false, identity: true),
                        AssignedMemberId = c.Int(nullable: false),
                        AssignmentId = c.Int(nullable: false),
                        UnproctoredAmount = c.Double(nullable: false),
                        ProctoredAmount = c.Double(nullable: false),
                        AssignedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MemberStudyHourAssignmentId)
                .ForeignKey("dbo.Members", t => t.AssignedMemberId, cascadeDelete: true)
                .ForeignKey("dbo.StudyHourAssignments", t => t.AssignmentId, cascadeDelete: true)
                .Index(t => new { t.AssignedMemberId, t.AssignmentId, t.UnproctoredAmount, t.ProctoredAmount }, unique: true, name: "IX_AssignedMemberAndStartAndEndAmount");
            
            CreateTable(
                "dbo.StudyHourAssignments",
                c => new
                    {
                        StudyHourAssignmentId = c.Int(nullable: false, identity: true),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.StudyHourAssignmentId)
                .Index(t => new { t.Start, t.End }, unique: true, name: "IX_StudyHourAssignmentDateTimeAmount");
            
            AddColumn("dbo.StudyHours", "AssignmentId", c => c.Int(nullable: false));
            AlterColumn("dbo.StudyHours", "ApproverId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.StudyHours", "StudyHourId");
            CreateIndex("dbo.StudyHours", new[] { "DateTimeStudied", "AssignmentId" }, unique: true, name: "IX_SubmitterAndDateTimeStudiedAndAssignment");
            CreateIndex("dbo.StudyHours", "ApproverId");
            AddForeignKey("dbo.StudyHours", "AssignmentId", "dbo.MemberStudyHourAssignments", "MemberStudyHourAssignmentId", cascadeDelete: true);
            DropColumn("dbo.StudyHours", "SubmittedBy");
            DropColumn("dbo.StudyHours", "RequiredStudyHours");
            DropColumn("dbo.StudyHours", "ProctoredStudyHours");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StudyHours", "ProctoredStudyHours", c => c.Int());
            AddColumn("dbo.StudyHours", "RequiredStudyHours", c => c.Int(nullable: false));
            AddColumn("dbo.StudyHours", "SubmittedBy", c => c.Int(nullable: false));
            DropForeignKey("dbo.StudyHours", "AssignmentId", "dbo.MemberStudyHourAssignments");
            DropForeignKey("dbo.MemberStudyHourAssignments", "AssignmentId", "dbo.StudyHourAssignments");
            DropForeignKey("dbo.MemberStudyHourAssignments", "AssignedMemberId", "dbo.Members");
            DropIndex("dbo.StudyHourAssignments", "IX_StudyHourAssignmentDateTimeAmount");
            DropIndex("dbo.MemberStudyHourAssignments", "IX_AssignedMemberAndStartAndEndAmount");
            DropIndex("dbo.StudyHours", new[] { "ApproverId" });
            DropIndex("dbo.StudyHours", "IX_SubmitterAndDateTimeStudiedAndAssignment");
            DropPrimaryKey("dbo.StudyHours");
            AlterColumn("dbo.StudyHours", "ApproverId", c => c.Int());
            DropColumn("dbo.StudyHours", "AssignmentId");
            DropTable("dbo.StudyHourAssignments");
            DropTable("dbo.MemberStudyHourAssignments");
            AddPrimaryKey("dbo.StudyHours", new[] { "StudyHourId", "SubmittedBy", "DateTimeStudied" });
            CreateIndex("dbo.StudyHours", "ApproverId");
            CreateIndex("dbo.StudyHours", "SubmittedBy");
            AddForeignKey("dbo.StudyHours", "SubmittedBy", "dbo.Members", "UserId");
        }
    }
}
