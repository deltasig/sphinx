namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdditionalActivityTracking : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Members", "CreatedOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.Members", "LastUpdatedOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.ClassesTaken", "CreatedOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.Events", "CreatedOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.SoberSignups", "CreatedOn", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SoberSignups", "CreatedOn");
            DropColumn("dbo.Events", "CreatedOn");
            DropColumn("dbo.ClassesTaken", "CreatedOn");
            DropColumn("dbo.Members", "LastUpdatedOn");
            DropColumn("dbo.Members", "CreatedOn");
        }
    }
}
