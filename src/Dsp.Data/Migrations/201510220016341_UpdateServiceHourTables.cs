namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateServiceHourTables : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Events", newName: "ServiceEvents");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ServiceEvents", newName: "Events");
        }
    }
}
