namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LatePlateLiberation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MealLatePlates", "MealToPeriodId", "dbo.MealToPeriods");
            DropForeignKey("dbo.MealLatePlates", "UserId", "dbo.Members");
            DropIndex("dbo.MealLatePlates", "IX_MealLatePlate");
            CreateTable(
                "dbo.MealPlates",
                c => new
                    {
                        MealLatePlateId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        PlateDateTime = c.DateTime(nullable: false),
                        SignedUpOn = c.DateTime(nullable: false),
                        Type = c.String(maxLength: 25),
                    })
                .PrimaryKey(t => t.MealLatePlateId)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            DropTable("dbo.MealLatePlates");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MealLatePlates",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        MealToPeriodId = c.Int(nullable: false),
                        MealLatePlateId = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.MealLatePlateId);
            
            DropForeignKey("dbo.MealPlates", "UserId", "dbo.Members");
            DropIndex("dbo.MealPlates", new[] { "UserId" });
            DropTable("dbo.MealPlates");
            CreateIndex("dbo.MealLatePlates", new[] { "UserId", "MealToPeriodId" }, unique: true, name: "IX_MealLatePlate");
            AddForeignKey("dbo.MealLatePlates", "UserId", "dbo.Members", "UserId", cascadeDelete: true);
            AddForeignKey("dbo.MealLatePlates", "MealToPeriodId", "dbo.MealToPeriods", "MealToPeriodId", cascadeDelete: true);
        }
    }
}
