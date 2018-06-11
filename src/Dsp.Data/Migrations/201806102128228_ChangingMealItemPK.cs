namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangingMealItemPK : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MealToItems", "MealItemId", "dbo.MealItems");
            DropForeignKey("dbo.MealVotes", "MealItemId", "dbo.MealItems");

            RenameColumn("dbo.MealItems", "MealItemId", "Id");

            AddForeignKey("dbo.MealToItems", "MealItemId", "dbo.MealItems", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MealVotes", "MealItemId", "dbo.MealItems", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.MealToItems", "MealItemId", "dbo.MealItems");
            DropForeignKey("dbo.MealVotes", "MealItemId", "dbo.MealItems");

            RenameColumn("dbo.MealItems", "Id", "MealItemId");

            AddForeignKey("dbo.MealVotes", "MealItemId", "dbo.MealItems", "MealItemId", cascadeDelete: true);
            AddForeignKey("dbo.MealToItems", "MealItemId", "dbo.MealItems", "MealItemId", cascadeDelete: true);
        }
    }
}
