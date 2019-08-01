namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReaddShirtSize : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Members", "ShirtSize", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Members", "ShirtSize");
        }
    }
}
