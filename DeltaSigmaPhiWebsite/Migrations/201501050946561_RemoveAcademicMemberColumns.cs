namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveAcademicMemberColumns : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Members", "PreviousSemesterGPA");
            DropColumn("dbo.Members", "CumulativeGPA");
            DropColumn("dbo.Members", "RemainingBalance");
            DropColumn("dbo.Members", "RequiredStudyHours");
            DropColumn("dbo.Members", "ProctoredStudyHours");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Members", "ProctoredStudyHours", c => c.Int());
            AddColumn("dbo.Members", "RequiredStudyHours", c => c.Int(nullable: false));
            AddColumn("dbo.Members", "RemainingBalance", c => c.Double());
            AddColumn("dbo.Members", "CumulativeGPA", c => c.Double());
            AddColumn("dbo.Members", "PreviousSemesterGPA", c => c.Double());
        }
    }
}
