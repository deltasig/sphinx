namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BroQuest2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Semesters", "DefaultQuestChallengers");
            AlterColumn("dbo.Members", "QuestChallengeSize", c => c.Int(defaultValue: 30));
            AlterColumn("dbo.Members", "MaxQuesters", c => c.Int(defaultValue: 1));
            AlterColumn("dbo.Semesters", "MinQuestChallengeMinutes", c => c.Int(defaultValue: 30));
            AlterColumn("dbo.Semesters", "MaxQuestChallengeMinutes", c => c.Int(defaultValue: 90));
        }
        
        public override void Down()
        {
            AddColumn("dbo.Semesters", "DefaultQuestChallengers", c => c.Int(nullable: false));
        }
    }
}
