namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIncidentReportIdentitySharing : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.IncidentReports", "ShareIdentity", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.IncidentReports", "ShareIdentity");
        }
    }
}
