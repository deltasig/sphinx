namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MealsTables : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MealsCooked", "MealId", "dbo.Meals");
            DropIndex("dbo.MealsCooked", new[] { "MealId" });
            CreateTable(
                "dbo.MealVotes",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        MealItemId = c.Int(nullable: false),
                        MealVoteId = c.Int(nullable: false, identity: true),
                        IsUpvote = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.MealVoteId)
                .ForeignKey("dbo.MealItems", t => t.MealItemId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.MealItemId }, unique: true, name: "IX_MealVote");
            
            CreateTable(
                "dbo.MealItems",
                c => new
                    {
                        MealItemId = c.Int(nullable: false, identity: true),
                        MealItemTypeId = c.Int(nullable: false),
                        Name = c.String(maxLength: 150),
                        IsGlutenFree = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.MealItemId)
                .ForeignKey("dbo.MealItemTypes", t => t.MealItemTypeId, cascadeDelete: true)
                .Index(t => t.MealItemTypeId);
            
            CreateTable(
                "dbo.MealItemTypes",
                c => new
                    {
                        MealItemTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.MealItemTypeId);
            
            CreateTable(
                "dbo.MealToItems",
                c => new
                    {
                        MealId = c.Int(nullable: false),
                        MealItemId = c.Int(nullable: false),
                        MealToItemId = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.MealToItemId)
                .ForeignKey("dbo.Meals", t => t.MealId, cascadeDelete: true)
                .ForeignKey("dbo.MealItems", t => t.MealItemId, cascadeDelete: true)
                .Index(t => new { t.MealId, t.MealItemId }, unique: true, name: "IX_MealToItem");
            
            CreateTable(
                "dbo.MealToPeriods",
                c => new
                    {
                        MealToPeriodId = c.Int(nullable: false, identity: true),
                        MealPeriodId = c.Int(nullable: false),
                        MealId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false, storeType: "date"),
                    })
                .PrimaryKey(t => t.MealToPeriodId)
                .ForeignKey("dbo.Meals", t => t.MealId, cascadeDelete: true)
                .ForeignKey("dbo.MealPeriods", t => t.MealPeriodId, cascadeDelete: true)
                .Index(t => t.MealPeriodId)
                .Index(t => t.MealId);
            
            CreateTable(
                "dbo.MealPeriods",
                c => new
                    {
                        MealPeriodId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MealPeriodId);
            
            DropColumn("dbo.Meals", "MealTitle");
            DropTable("dbo.MealsCooked");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MealsCooked",
                c => new
                    {
                        ServingId = c.Int(nullable: false, identity: true),
                        DateServed = c.DateTime(nullable: false, storeType: "date"),
                        MealId = c.Int(nullable: false),
                        Lunch = c.Boolean(),
                        Dinner = c.Boolean(),
                    })
                .PrimaryKey(t => t.ServingId);
            
            AddColumn("dbo.Meals", "MealTitle", c => c.String(nullable: false, maxLength: 100));
            DropForeignKey("dbo.MealVotes", "UserId", "dbo.Members");
            DropForeignKey("dbo.MealVotes", "MealItemId", "dbo.MealItems");
            DropForeignKey("dbo.MealToItems", "MealItemId", "dbo.MealItems");
            DropForeignKey("dbo.MealToPeriods", "MealPeriodId", "dbo.MealPeriods");
            DropForeignKey("dbo.MealToPeriods", "MealId", "dbo.Meals");
            DropForeignKey("dbo.MealToItems", "MealId", "dbo.Meals");
            DropForeignKey("dbo.MealItems", "MealItemTypeId", "dbo.MealItemTypes");
            DropIndex("dbo.MealToPeriods", new[] { "MealId" });
            DropIndex("dbo.MealToPeriods", new[] { "MealPeriodId" });
            DropIndex("dbo.MealToItems", "IX_MealToItem");
            DropIndex("dbo.MealItems", new[] { "MealItemTypeId" });
            DropIndex("dbo.MealVotes", "IX_MealVote");
            DropTable("dbo.MealPeriods");
            DropTable("dbo.MealToPeriods");
            DropTable("dbo.MealToItems");
            DropTable("dbo.MealItemTypes");
            DropTable("dbo.MealItems");
            DropTable("dbo.MealVotes");
            CreateIndex("dbo.MealsCooked", "MealId");
            AddForeignKey("dbo.MealsCooked", "MealId", "dbo.Meals", "MealId");
        }
    }
}
