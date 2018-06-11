namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangingMealToPeriodPK : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.MealToPeriods", "MealToPeriodId", "Id");
        }

        public override void Down()
        {
            RenameColumn("dbo.MealToPeriods", "Id", "MealToPeriodId");
        }
    }
}
