namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LeaderPKRefactoring : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Leaders");
            AddPrimaryKey("dbo.Leaders", new[] { "UserId", "LeaderId", "SemesterId" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Leaders");
            AddPrimaryKey("dbo.Leaders", new[] { "UserId", "LeaderId" });
        }
    }
}
