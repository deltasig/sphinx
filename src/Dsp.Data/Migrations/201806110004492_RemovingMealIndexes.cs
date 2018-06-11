namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovingMealIndexes : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.MealItemVotes", "IX_MealVote");
            DropIndex("dbo.MealsToItems", "IX_MealToItem");
            DropIndex("dbo.MealsToPeriods", "IX_MealToPeriod");
            CreateIndex("dbo.MealItemVotes", "UserId");
            CreateIndex("dbo.MealItemVotes", "MealItemId");
            CreateIndex("dbo.MealsToItems", "MealId");
            CreateIndex("dbo.MealsToItems", "MealItemId");
            CreateIndex("dbo.MealsToPeriods", "MealPeriodId");
            CreateIndex("dbo.MealsToPeriods", "MealId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.MealsToPeriods", new[] { "MealId" });
            DropIndex("dbo.MealsToPeriods", new[] { "MealPeriodId" });
            DropIndex("dbo.MealsToItems", new[] { "MealItemId" });
            DropIndex("dbo.MealsToItems", new[] { "MealId" });
            DropIndex("dbo.MealItemVotes", new[] { "MealItemId" });
            DropIndex("dbo.MealItemVotes", new[] { "UserId" });
            CreateIndex("dbo.MealsToPeriods", new[] { "MealPeriodId", "MealId", "Date" }, unique: true, name: "IX_MealToPeriod");
            CreateIndex("dbo.MealsToItems", new[] { "MealId", "MealItemId" }, unique: true, name: "IX_MealToItem");
            CreateIndex("dbo.MealItemVotes", new[] { "UserId", "MealItemId" }, unique: true, name: "IX_MealVote");
        }
    }
}
