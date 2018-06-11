namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangingMealPK : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MealToItems", "MealId", "dbo.Meals");
            DropForeignKey("dbo.MealToPeriods", "MealId", "dbo.Meals");

            RenameColumn("dbo.Meals", "MealId", "Id");

            AddForeignKey("dbo.MealToItems", "MealId", "dbo.Meals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MealToPeriods", "MealId", "dbo.Meals", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.MealToPeriods", "MealId", "dbo.Meals");
            DropForeignKey("dbo.MealToItems", "MealId", "dbo.Meals");

            RenameColumn("dbo.Meals", "Id", "MealId");

            AddForeignKey("dbo.MealToPeriods", "MealId", "dbo.Meals", "MealId", cascadeDelete: true);
            AddForeignKey("dbo.MealToItems", "MealId", "dbo.Meals", "MealId", cascadeDelete: true);
        }
    }
}
