namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MessedUpSubmittedOnType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ScholarshipSubmissions", "SubmittedOn", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ScholarshipSubmissions", "SubmittedOn", c => c.Boolean(nullable: false));
        }
    }
}
