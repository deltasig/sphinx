namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rooms : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RoomToMembers",
                c => new
                    {
                        RoomToMemberId = c.Int(nullable: false, identity: true),
                        RoomId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        MovedIn = c.DateTime(nullable: false),
                        MovedOut = c.DateTime(),
                    })
                .PrimaryKey(t => t.RoomToMemberId)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Rooms", t => t.RoomId, cascadeDelete: true)
                .Index(t => t.RoomId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        RoomId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.RoomId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoomToMembers", "RoomId", "dbo.Rooms");
            DropForeignKey("dbo.RoomToMembers", "UserId", "dbo.Members");
            DropIndex("dbo.RoomToMembers", new[] { "UserId" });
            DropIndex("dbo.RoomToMembers", new[] { "RoomId" });
            DropTable("dbo.Rooms");
            DropTable("dbo.RoomToMembers");
        }
    }
}
