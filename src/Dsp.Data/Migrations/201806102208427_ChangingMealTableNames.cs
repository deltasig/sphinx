namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangingMealTableNames : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.MealToItems", newName: "MealsToItems");
            RenameTable(name: "dbo.MealToPeriods", newName: "MealsToPeriods");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.MealsToPeriods", newName: "MealToPeriods");
            RenameTable(name: "dbo.MealsToItems", newName: "MealToItems");
        }
    }
}
