namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveOldSoberType : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SoberSignups", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SoberSignups", "Type", c => c.Int(nullable: false));
        }
    }
}
