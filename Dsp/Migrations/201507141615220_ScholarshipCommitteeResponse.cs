namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ScholarshipCommitteeResponse : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScholarshipSubmissions", "CommitteeResponse", c => c.String());
            AddColumn("dbo.ScholarshipSubmissions", "CommitteeRespondedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScholarshipSubmissions", "CommitteeRespondedOn");
            DropColumn("dbo.ScholarshipSubmissions", "CommitteeResponse");
        }
    }
}
