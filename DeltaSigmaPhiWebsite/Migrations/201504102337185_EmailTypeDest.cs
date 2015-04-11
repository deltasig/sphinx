namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmailTypeDest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmailTypes", "Destination", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmailTypes", "Destination");
        }
    }
}
