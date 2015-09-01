namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StudyHours1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StudyHours", "SignedOutOn", c => c.DateTime(nullable: false));
            AlterColumn("dbo.StudySessions", "Location", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StudySessions", "Location", c => c.String());
            AlterColumn("dbo.StudyHours", "SignedOutOn", c => c.DateTime());
        }
    }
}
