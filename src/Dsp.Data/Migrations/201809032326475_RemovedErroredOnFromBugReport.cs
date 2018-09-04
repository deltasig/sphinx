namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedErroredOnFromBugReport : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.BugReports", "ErroredOn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BugReports", "ErroredOn", c => c.DateTime(nullable: false));
        }
    }
}
