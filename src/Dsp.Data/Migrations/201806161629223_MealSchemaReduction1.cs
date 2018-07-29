namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MealSchemaReduction1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MealItemsToPeriods",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    MealPeriodId = c.Int(nullable: false),
                    MealItemId = c.Int(nullable: false),
                    Date = c.DateTime(nullable: false, storeType: "date"),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MealPeriods", t => t.MealPeriodId, cascadeDelete: true)
                .ForeignKey("dbo.MealItems", t => t.MealItemId, cascadeDelete: true)
                .Index(t => new { t.MealPeriodId, t.MealItemId, t.Date }, unique: true, name: "IX_MealPeriod_MealItem_Date");
        }

        public override void Down()
        {
            DropForeignKey("dbo.MealItemsToPeriods", "MealItemId", "dbo.MealItems");
            DropForeignKey("dbo.MealItemsToPeriods", "MealPeriodId", "dbo.MealPeriods");
            DropIndex("dbo.MealItemsToPeriods", "IX_MealPeriod_MealItem_Date");
            DropTable("dbo.MealItemsToPeriods");
        }
    }
}
