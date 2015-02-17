namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Majors : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MemberMajors", "MajorId", "dbo.Majors");
            DropForeignKey("dbo.MemberMajors", "UserId", "dbo.Members");
            DropIndex("dbo.MemberMajors", new[] { "MajorId" });
            DropIndex("dbo.MemberMajors", new[] { "UserId" });
            CreateTable(
                "dbo.MajorToMembers",
                c => new
                    {
                        MajorId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        MajorToMemberId = c.Int(nullable: false, identity: true),
                        DegreeLevel = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MajorToMemberId)
                .ForeignKey("dbo.Majors", t => t.MajorId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.MajorId, t.UserId }, unique: true, name: "IX_MajorToMember");
            
            DropTable("dbo.MemberMajors");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MemberMajors",
                c => new
                    {
                        MajorId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.MajorId, t.UserId });
            
            DropForeignKey("dbo.MajorToMembers", "UserId", "dbo.Members");
            DropForeignKey("dbo.MajorToMembers", "MajorId", "dbo.Majors");
            DropIndex("dbo.MajorToMembers", "IX_MajorToMember");
            DropTable("dbo.MajorToMembers");
            CreateIndex("dbo.MemberMajors", "UserId");
            CreateIndex("dbo.MemberMajors", "MajorId");
            AddForeignKey("dbo.MemberMajors", "UserId", "dbo.Members", "UserId", cascadeDelete: true);
            AddForeignKey("dbo.MemberMajors", "MajorId", "dbo.Majors", "MajorId", cascadeDelete: true);
        }
    }
}
