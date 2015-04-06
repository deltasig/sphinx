namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SoberTypeDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SoberTypes", "Description", c => c.String(maxLength: 2000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SoberTypes", "Description");
        }
    }
}
