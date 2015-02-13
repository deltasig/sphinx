namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClassFileVotes : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ClassFiles", "UserId", "dbo.Members");
            CreateTable(
                "dbo.ClassFileVotes",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        ClassFileId = c.Int(nullable: false),
                        ClassFileVoteId = c.Int(nullable: false, identity: true),
                        IsUpvote = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ClassFileVoteId)
                .ForeignKey("dbo.ClassFiles", t => t.ClassFileId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => new { t.UserId, t.ClassFileId }, unique: true, name: "IX_ClassFileVote");
            
            AddForeignKey("dbo.ClassFiles", "UserId", "dbo.Members", "UserId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClassFiles", "UserId", "dbo.Members");
            DropForeignKey("dbo.ClassFileVotes", "UserId", "dbo.Members");
            DropForeignKey("dbo.ClassFileVotes", "ClassFileId", "dbo.ClassFiles");
            DropIndex("dbo.ClassFileVotes", "IX_ClassFileVote");
            DropTable("dbo.ClassFileVotes");
            AddForeignKey("dbo.ClassFiles", "UserId", "dbo.Members", "UserId", cascadeDelete: true);
        }
    }
}
