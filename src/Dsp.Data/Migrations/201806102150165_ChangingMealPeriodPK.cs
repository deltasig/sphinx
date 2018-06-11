namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangingMealPeriodPK : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MealToPeriods", "MealPeriodId", "dbo.MealPeriods");

            RenameColumn("dbo.MealPeriods", "MealPeriodId", "Id");

            AddForeignKey("dbo.MealToPeriods", "MealPeriodId", "dbo.MealPeriods", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
            AddColumn("dbo.MealPeriods", "MealPeriodId", c => c.Int(nullable: false, identity: true));

            RenameColumn("dbo.MealPeriods", "Id", "MealPeriodId");

            AddForeignKey("dbo.MealToPeriods", "MealPeriodId", "dbo.MealPeriods", "MealPeriodId", cascadeDelete: true);
        }
    }
}
