namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Philanthropy3 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Philanthropies", newName: "Causes");
            RenameColumn(table: "dbo.Fundraisers", name: "PhilanthropyId", newName: "CauseId");
            RenameIndex(table: "dbo.Fundraisers", name: "IX_PhilanthropyId", newName: "IX_CauseId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Fundraisers", name: "IX_CauseId", newName: "IX_PhilanthropyId");
            RenameColumn(table: "dbo.Fundraisers", name: "CauseId", newName: "PhilanthropyId");
            RenameTable(name: "dbo.Causes", newName: "Philanthropies");
        }
    }
}
