namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BugReportRequiredProperties : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BugReports", "Description", c => c.String(nullable: false, maxLength: 2500));
            AlterColumn("dbo.BugReports", "UrlWithProblem", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.BugReports", "UrlWithProblem", c => c.String(maxLength: 100));
            AlterColumn("dbo.BugReports", "Description", c => c.String(maxLength: 2500));
        }
    }
}
