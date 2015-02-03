namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MealItemOrderAndClassTakenSummer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClassesTaken", "IsSummerClass", c => c.Boolean(nullable: false));
            AddColumn("dbo.MealToItems", "DisplayOrder", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MealToItems", "DisplayOrder");
            DropColumn("dbo.ClassesTaken", "IsSummerClass");
        }
    }
}
