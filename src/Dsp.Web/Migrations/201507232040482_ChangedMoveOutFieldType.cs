namespace Dsp.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedMoveOutFieldType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RoomToMembers", "MovedOut", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RoomToMembers", "MovedOut", c => c.DateTime());
        }
    }
}
