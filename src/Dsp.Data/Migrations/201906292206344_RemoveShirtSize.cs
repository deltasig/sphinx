namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveShirtSize : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Members", "ShirtSize");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Members", "ShirtSize", c => c.String());
        }
    }
}
