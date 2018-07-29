namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MealSchemaReduction3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MealsToItems", "MealId", "dbo.Meals");
            DropForeignKey("dbo.MealsToPeriods", "MealId", "dbo.Meals");
            DropForeignKey("dbo.MealsToPeriods", "MealPeriodId", "dbo.MealPeriods");
            DropForeignKey("dbo.MealsToItems", "MealItemId", "dbo.MealItems");
            DropIndex("dbo.MealsToPeriods", "IX_MealPeriod_Meal_Date");
            DropIndex("dbo.MealsToItems", "IX_Meal_MealItem");
            DropTable("dbo.MealsToPeriods");
            DropTable("dbo.MealsToItems");
            DropTable("dbo.Meals");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.Meals",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.MealsToItems",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    MealId = c.Int(nullable: false),
                    MealItemId = c.Int(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.MealsToPeriods",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    MealPeriodId = c.Int(nullable: false),
                    MealId = c.Int(nullable: false),
                    Date = c.DateTime(nullable: false, storeType: "date"),
                })
                .PrimaryKey(t => t.Id);

            CreateIndex("dbo.MealsToItems", new[] { "MealId", "MealItemId" }, unique: true, name: "IX_Meal_MealItem");
            CreateIndex("dbo.MealsToPeriods", new[] { "MealPeriodId", "MealId", "Date" }, unique: true, name: "IX_MealPeriod_Meal_Date");
            AddForeignKey("dbo.MealsToItems", "MealItemId", "dbo.MealItems", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MealsToPeriods", "MealPeriodId", "dbo.MealPeriods", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MealsToPeriods", "MealId", "dbo.Meals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MealsToItems", "MealId", "dbo.Meals", "Id", cascadeDelete: true);
        }
    }
}
