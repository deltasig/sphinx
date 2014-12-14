namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FallTransitionDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Semesters", "FallTransitionDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Semesters", "FallTransitionDate");
        }
    }
}
