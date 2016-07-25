namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BroQuest3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuestChallenges", "PeriodId", "dbo.QuestPeriods");
            DropIndex("dbo.QuestChallenges", new[] { "PeriodId" });
            AddColumn("dbo.Semesters", "QuestingBeginsOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.Semesters", "QuestingEndsOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.QuestChallenges", "SemesterId", c => c.Int(nullable: false));
            CreateIndex("dbo.QuestChallenges", "SemesterId");
            AddForeignKey("dbo.QuestChallenges", "SemesterId", "dbo.Semesters", "SemesterId", cascadeDelete: true);
            DropColumn("dbo.QuestChallenges", "PeriodId");
            DropTable("dbo.QuestPeriods");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.QuestPeriods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BeginsOn = c.DateTime(nullable: false),
                        EndsOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.QuestChallenges", "PeriodId", c => c.Int(nullable: false));
            DropForeignKey("dbo.QuestChallenges", "SemesterId", "dbo.Semesters");
            DropIndex("dbo.QuestChallenges", new[] { "SemesterId" });
            DropColumn("dbo.QuestChallenges", "SemesterId");
            DropColumn("dbo.Semesters", "QuestingEndsOn");
            DropColumn("dbo.Semesters", "QuestingBeginsOn");
            CreateIndex("dbo.QuestChallenges", "PeriodId");
            AddForeignKey("dbo.QuestChallenges", "PeriodId", "dbo.QuestPeriods", "Id", cascadeDelete: true);
        }
    }
}
