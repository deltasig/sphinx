namespace Dsp.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRoomField : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Members", "Room");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Members", "Room", c => c.Int());
        }
    }
}
