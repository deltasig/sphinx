namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ScholarshipEmailColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScholarshipSubmissions", "Email", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ScholarshipSubmissions", "Email");
        }
    }
}
