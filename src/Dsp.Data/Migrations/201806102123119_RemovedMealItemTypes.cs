namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedMealItemTypes : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MealItems", "MealItemTypeId", "dbo.MealItemTypes");
            DropIndex("dbo.MealItems", new[] { "MealItemTypeId" });
            DropColumn("dbo.MealItems", "MealItemTypeId");
            DropTable("dbo.MealItemTypes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MealItemTypes",
                c => new
                    {
                        MealItemTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.MealItemTypeId);
            
            AddColumn("dbo.MealItems", "MealItemTypeId", c => c.Int(nullable: false));
            CreateIndex("dbo.MealItems", "MealItemTypeId");
            AddForeignKey("dbo.MealItems", "MealItemTypeId", "dbo.MealItemTypes", "MealItemTypeId", cascadeDelete: true);
        }
    }
}
