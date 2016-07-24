namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BroQuest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestChallenges",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PeriodId = c.Int(nullable: false),
                        MemberId = c.Int(nullable: false),
                        BeginsOn = c.DateTime(),
                        EndsOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuestPeriods", t => t.PeriodId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.MemberId, cascadeDelete: true)
                .Index(t => t.PeriodId)
                .Index(t => t.MemberId);
            
            CreateTable(
                "dbo.QuestCompletions",
                c => new
                    {
                        ChallengeId = c.Int(nullable: false),
                        NewMemberId = c.Int(nullable: false),
                        Id = c.Int(nullable: false, identity: true),
                        IsVerified = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.NewMemberId)
                .ForeignKey("dbo.QuestChallenges", t => t.ChallengeId, cascadeDelete: true)
                .Index(t => new { t.ChallengeId, t.NewMemberId }, unique: true, name: "IX_QuestCompletion");
            
            CreateTable(
                "dbo.QuestPeriods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BeginsOn = c.DateTime(nullable: false),
                        EndsOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Members", "QuestChallengeSize", c => c.Int(nullable: false));
            AddColumn("dbo.Members", "MaxQuesters", c => c.Int(nullable: false));
            AddColumn("dbo.Semesters", "MinQuestChallengeMinutes", c => c.Int(nullable: false));
            AddColumn("dbo.Semesters", "MaxQuestChallengeMinutes", c => c.Int(nullable: false));
            AddColumn("dbo.Semesters", "DefaultQuestChallengers", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuestChallenges", "MemberId", "dbo.Members");
            DropForeignKey("dbo.QuestChallenges", "PeriodId", "dbo.QuestPeriods");
            DropForeignKey("dbo.QuestCompletions", "ChallengeId", "dbo.QuestChallenges");
            DropForeignKey("dbo.QuestCompletions", "NewMemberId", "dbo.Members");
            DropIndex("dbo.QuestCompletions", "IX_QuestCompletion");
            DropIndex("dbo.QuestChallenges", new[] { "MemberId" });
            DropIndex("dbo.QuestChallenges", new[] { "PeriodId" });
            DropColumn("dbo.Semesters", "DefaultQuestChallengers");
            DropColumn("dbo.Semesters", "MaxQuestChallengeMinutes");
            DropColumn("dbo.Semesters", "MinQuestChallengeMinutes");
            DropColumn("dbo.Members", "MaxQuesters");
            DropColumn("dbo.Members", "QuestChallengeSize");
            DropTable("dbo.QuestPeriods");
            DropTable("dbo.QuestCompletions");
            DropTable("dbo.QuestChallenges");
        }
    }
}
