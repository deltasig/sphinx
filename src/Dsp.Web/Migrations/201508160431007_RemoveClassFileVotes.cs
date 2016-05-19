namespace Dsp.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveClassFileVotes : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ClassFileVotes", "ClassFileId", "dbo.ClassFiles");
            DropForeignKey("dbo.ClassFileVotes", "UserId", "dbo.Members");
            DropIndex("dbo.ClassFileVotes", "IX_ClassFileVote");
            DropTable("dbo.ClassFileVotes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ClassFileVotes",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        ClassFileId = c.Int(nullable: false),
                        ClassFileVoteId = c.Int(nullable: false, identity: true),
                        IsUpvote = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ClassFileVoteId);
            
            CreateIndex("dbo.ClassFileVotes", new[] { "UserId", "ClassFileId" }, unique: true, name: "IX_ClassFileVote");
            AddForeignKey("dbo.ClassFileVotes", "UserId", "dbo.Members", "UserId", cascadeDelete: true);
            AddForeignKey("dbo.ClassFileVotes", "ClassFileId", "dbo.ClassFiles", "ClassFileId", cascadeDelete: true);
        }
    }
}
