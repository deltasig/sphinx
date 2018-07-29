namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MealSchemaReduction2 : DbMigration
    {
        public override void Up()
        {
            var dataMigrationQuery = @"
            INSERT INTO dbo.MealItemsToPeriods
            SELECT mp.MealPeriodId, mi.MealItemId, mp.Date
            FROM dbo.MealsToPeriods as mp 
	            INNER JOIN dbo.Meals as m on mp.MealId = m.Id
	            INNER JOIN dbo.MealsToItems as mi on m.Id = mi.MealId";
            Sql(dataMigrationQuery);
        }

        public override void Down()
        {
        }
    }
}
