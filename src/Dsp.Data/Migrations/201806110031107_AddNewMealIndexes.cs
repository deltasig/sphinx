namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNewMealIndexes : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.MealItemVotes", new[] { "UserId" });
            DropIndex("dbo.MealItemVotes", new[] { "MealItemId" });
            DropIndex("dbo.MealsToItems", new[] { "MealId" });
            DropIndex("dbo.MealsToItems", new[] { "MealItemId" });
            DropIndex("dbo.MealsToPeriods", new[] { "MealPeriodId" });
            DropIndex("dbo.MealsToPeriods", new[] { "MealId" });
            CreateIndex("dbo.MealItemVotes", new[] { "UserId", "MealItemId" }, unique: true, name: "IX_User_MealItem");
            CreateIndex("dbo.MealsToItems", new[] { "MealId", "MealItemId" }, unique: true, name: "IX_Meal_MealItem");
            CreateIndex("dbo.MealsToPeriods", new[] { "MealPeriodId", "MealId", "Date" }, unique: true, name: "IX_MealPeriod_Meal_Date");
        }
        
        public override void Down()
        {
            DropIndex("dbo.MealsToPeriods", "IX_MealPeriod_Meal_Date");
            DropIndex("dbo.MealsToItems", "IX_Meal_MealItem");
            DropIndex("dbo.MealItemVotes", "IX_User_MealItem");
            CreateIndex("dbo.MealsToPeriods", "MealId");
            CreateIndex("dbo.MealsToPeriods", "MealPeriodId");
            CreateIndex("dbo.MealsToItems", "MealItemId");
            CreateIndex("dbo.MealsToItems", "MealId");
            CreateIndex("dbo.MealItemVotes", "MealItemId");
            CreateIndex("dbo.MealItemVotes", "UserId");
        }
    }
}
