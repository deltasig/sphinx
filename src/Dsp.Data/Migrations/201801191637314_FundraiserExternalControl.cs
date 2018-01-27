namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FundraiserExternalControl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Fundraisers", "IsPublic", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Fundraisers", "IsPublic");
        }
    }
}
