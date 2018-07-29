namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CachingMealVotes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MealItems", "Upvotes", c => c.Int(nullable: false));
            AddColumn("dbo.MealItems", "Downvotes", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MealItems", "Downvotes");
            DropColumn("dbo.MealItems", "Upvotes");
        }
    }
}
