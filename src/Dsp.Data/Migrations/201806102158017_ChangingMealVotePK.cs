namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangingMealVotePK : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.MealVotes", "MealVoteId", "Id");
        }

        public override void Down()
        {
            RenameColumn("dbo.MealVotes", "Id", "MealVoteId");
        }
    }
}
