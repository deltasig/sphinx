namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LatePlates : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MealLatePlates",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        MealToPeriodId = c.Int(nullable: false),
                        MealLatePlateId = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.MealLatePlateId)
                .ForeignKey("dbo.MealToPeriods", t => t.MealToPeriodId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.MealToPeriodId }, unique: true, name: "IX_MealLatePlate");
            
            AlterColumn("dbo.MealItems", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.MealItemTypes", "Name", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.MealPeriods", "Name", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MealLatePlates", "UserId", "dbo.Members");
            DropForeignKey("dbo.MealLatePlates", "MealToPeriodId", "dbo.MealToPeriods");
            DropIndex("dbo.MealLatePlates", "IX_MealLatePlate");
            AlterColumn("dbo.MealPeriods", "Name", c => c.String(maxLength: 100));
            AlterColumn("dbo.MealItemTypes", "Name", c => c.String(maxLength: 50));
            AlterColumn("dbo.MealItems", "Name", c => c.String(maxLength: 150));
            DropTable("dbo.MealLatePlates");
        }
    }
}
