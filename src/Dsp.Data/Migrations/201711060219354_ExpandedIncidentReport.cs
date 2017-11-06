namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExpandedIncidentReport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.IncidentReports", "InvestigationNotes", c => c.String(nullable: false, maxLength: 3000));
            RenameColumn("dbo.IncidentReports", "IncidentId", "Id");
        }
        
        public override void Down()
        {
            DropColumn("dbo.IncidentReports", "InvestigationNotes");
            RenameColumn("dbo.IncidentReports", "Id", "IncidentId");
        }
    }
}
