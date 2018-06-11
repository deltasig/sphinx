namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangingMealToItemPK : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.MealToItems", "MealToItemId", "Id");
        }

        public override void Down()
        {
            RenameColumn("dbo.MealToItems", "Id", "MealToItemId");
        }
    }
}
