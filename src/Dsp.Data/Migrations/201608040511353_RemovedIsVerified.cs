namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedIsVerified : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.QuestCompletions", "IsVerified");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuestCompletions", "IsVerified", c => c.Boolean(nullable: false));
        }
    }
}
