namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangingMealPlatePK : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.MealPlates", "MealPlateId", "Id");
        }

        public override void Down()
        {
            RenameColumn("dbo.MealPlates", "Id", "MealPlateId");
        }
    }
}
