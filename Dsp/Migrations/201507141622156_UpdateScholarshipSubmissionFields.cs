namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateScholarshipSubmissionFields : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ScholarshipSubmissions", "StudentNumber", c => c.String(nullable: false, maxLength: 15));
            AlterColumn("dbo.ScholarshipSubmissions", "PhoneNumber", c => c.String(nullable: false, maxLength: 15));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ScholarshipSubmissions", "PhoneNumber", c => c.String(nullable: false));
            AlterColumn("dbo.ScholarshipSubmissions", "StudentNumber", c => c.String(nullable: false));
        }
    }
}
