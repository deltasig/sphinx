namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rooms1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Rooms", "MaxCapacity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Rooms", "MaxCapacity");
        }
    }
}
