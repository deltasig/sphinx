namespace Dsp.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StudyHoursUpdate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StudyHours", "DurationMinutes", c => c.Double());
            AlterColumn("dbo.StudySessions", "Location", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StudySessions", "Location", c => c.String());
            AlterColumn("dbo.StudyHours", "DurationMinutes", c => c.Double(nullable: false));
        }
    }
}
