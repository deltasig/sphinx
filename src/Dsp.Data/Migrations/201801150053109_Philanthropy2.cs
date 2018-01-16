namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Philanthropy2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Fundraisers", "BeginsOn", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Fundraisers", "BeginsOn", c => c.DateTime());
        }
    }
}
