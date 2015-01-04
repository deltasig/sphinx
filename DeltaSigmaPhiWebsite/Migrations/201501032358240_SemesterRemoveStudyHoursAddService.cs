namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SemesterRemoveStudyHoursAddService : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Semesters", "MinimumServiceHours", c => c.Int(nullable: false, defaultValue: 10));
            DropColumn("dbo.Semesters", "StudyHourStart");
            DropColumn("dbo.Semesters", "StudyHourEnd");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Semesters", "StudyHourEnd", c => c.DateTime(nullable: false));
            AddColumn("dbo.Semesters", "StudyHourStart", c => c.DateTime(nullable: false));
            DropColumn("dbo.Semesters", "MinimumServiceHours");
        }
    }
}
