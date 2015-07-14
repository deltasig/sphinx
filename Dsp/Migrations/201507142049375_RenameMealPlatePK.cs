namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameMealPlatePK : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.MealPlates");
            DropColumn("dbo.MealPlates", "MealLatePlateId");
            AddColumn("dbo.MealPlates", "MealPlateId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.MealPlates", "MealPlateId");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.MealPlates");
            DropColumn("dbo.MealPlates", "MealPlateId");
            AddColumn("dbo.MealPlates", "MealLatePlateId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.MealPlates", "MealLatePlateId");
        }
    }
}
