namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestChallengeIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.QuestChallenges", new[] { "SemesterId" });
            DropIndex("dbo.QuestChallenges", new[] { "MemberId" });
            AlterColumn("dbo.QuestChallenges", "BeginsOn", c => c.DateTime(nullable: false));
            AlterColumn("dbo.QuestChallenges", "EndsOn", c => c.DateTime(nullable: false));
            CreateIndex("dbo.QuestChallenges", new[] { "SemesterId", "MemberId", "BeginsOn", "EndsOn" }, unique: true, name: "IX_QuestChallenge");
        }
        
        public override void Down()
        {
            DropIndex("dbo.QuestChallenges", "IX_QuestChallenge");
            AlterColumn("dbo.QuestChallenges", "EndsOn", c => c.DateTime());
            AlterColumn("dbo.QuestChallenges", "BeginsOn", c => c.DateTime());
            CreateIndex("dbo.QuestChallenges", "MemberId");
            CreateIndex("dbo.QuestChallenges", "SemesterId");
        }
    }
}
