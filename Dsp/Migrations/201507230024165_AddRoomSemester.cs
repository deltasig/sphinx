namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRoomSemester : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Rooms", "SemesterId", c => c.Int(nullable: false));
            CreateIndex("dbo.Rooms", new[] { "SemesterId", "Name" }, unique: true, name: "IX_RoomSemesterName");
            AddForeignKey("dbo.Rooms", "SemesterId", "dbo.Semesters", "SemesterId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Rooms", "SemesterId", "dbo.Semesters");
            DropIndex("dbo.Rooms", "IX_RoomSemesterName");
            DropColumn("dbo.Rooms", "SemesterId");
        }
    }
}
