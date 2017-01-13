namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedBroQuest : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuestCompletions", "NewMemberId", "dbo.Members");
            DropForeignKey("dbo.QuestCompletions", "ChallengeId", "dbo.QuestChallenges");
            DropForeignKey("dbo.QuestChallenges", "SemesterId", "dbo.Semesters");
            DropForeignKey("dbo.QuestChallenges", "MemberId", "dbo.Members");
            DropIndex("dbo.QuestChallenges", "IX_QuestChallenge");
            DropIndex("dbo.QuestCompletions", "IX_QuestCompletion");
            DropColumn("dbo.Members", "QuestChallengeSize");
            DropColumn("dbo.Members", "MaxQuesters");
            DropColumn("dbo.Semesters", "MinQuestChallengeMinutes");
            DropColumn("dbo.Semesters", "MaxQuestChallengeMinutes");
            DropColumn("dbo.Semesters", "QuestingBeginsOn");
            DropColumn("dbo.Semesters", "QuestingEndsOn");
            DropTable("dbo.QuestChallenges");
            DropTable("dbo.QuestCompletions");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.QuestCompletions",
                c => new
                    {
                        ChallengeId = c.Int(nullable: false),
                        NewMemberId = c.Int(nullable: false),
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.QuestChallenges",
                c => new
                    {
                        SemesterId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                        BeginsOn = c.DateTime(nullable: false),
                        EndsOn = c.DateTime(nullable: false),
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Semesters", "QuestingEndsOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.Semesters", "QuestingBeginsOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.Semesters", "MaxQuestChallengeMinutes", c => c.Int(nullable: false));
            AddColumn("dbo.Semesters", "MinQuestChallengeMinutes", c => c.Int(nullable: false));
            AddColumn("dbo.Members", "MaxQuesters", c => c.Int(nullable: false));
            AddColumn("dbo.Members", "QuestChallengeSize", c => c.Int(nullable: false));
            CreateIndex("dbo.QuestCompletions", new[] { "ChallengeId", "NewMemberId" }, unique: true, name: "IX_QuestCompletion");
            CreateIndex("dbo.QuestChallenges", new[] { "SemesterId", "MemberId", "BeginsOn", "EndsOn" }, unique: true, name: "IX_QuestChallenge");
            AddForeignKey("dbo.QuestChallenges", "MemberId", "dbo.Members", "UserId", cascadeDelete: true);
            AddForeignKey("dbo.QuestChallenges", "SemesterId", "dbo.Semesters", "SemesterId", cascadeDelete: true);
            AddForeignKey("dbo.QuestCompletions", "ChallengeId", "dbo.QuestChallenges", "Id", cascadeDelete: true);
            AddForeignKey("dbo.QuestCompletions", "NewMemberId", "dbo.Members", "UserId");
        }
    }
}
