namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MealToPeriodIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.MealToPeriods", new[] { "MealPeriodId" });
            DropIndex("dbo.MealToPeriods", new[] { "MealId" });
            CreateIndex("dbo.MealToPeriods", new[] { "MealPeriodId", "MealId", "Date" }, unique: true, name: "IX_MealToPeriod");
        }
        
        public override void Down()
        {
            DropIndex("dbo.MealToPeriods", "IX_MealToPeriod");
            CreateIndex("dbo.MealToPeriods", "MealId");
            CreateIndex("dbo.MealToPeriods", "MealPeriodId");
        }
    }
}
