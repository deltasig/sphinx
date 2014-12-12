namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PositionUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Positions", "Description", c => c.String(maxLength: 100));
            AddColumn("dbo.Positions", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.Positions", "DisplayOrder", c => c.Int(nullable: false));
            AddColumn("dbo.Positions", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Positions", "IsPublic", c => c.Boolean(nullable: false));
            AddColumn("dbo.Positions", "CanBeRemoved", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Positions", "PositionName", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Positions", "IsExecutive", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Positions", "IsElected", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Positions", "IsElected", c => c.Boolean());
            AlterColumn("dbo.Positions", "IsExecutive", c => c.Boolean());
            AlterColumn("dbo.Positions", "PositionName", c => c.String(maxLength: 50));
            DropColumn("dbo.Positions", "CanBeRemoved");
            DropColumn("dbo.Positions", "IsPublic");
            DropColumn("dbo.Positions", "IsDisabled");
            DropColumn("dbo.Positions", "DisplayOrder");
            DropColumn("dbo.Positions", "Type");
            DropColumn("dbo.Positions", "Description");
        }
    }
}
