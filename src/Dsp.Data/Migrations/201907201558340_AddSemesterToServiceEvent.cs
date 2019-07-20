namespace Dsp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddSemesterToServiceEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ServiceEvents", "SemesterId", c => c.Int());
            Sql(@"
            UPDATE ServiceEvents
            SET SemesterId=(
                SELECT TOP 1 SemesterId
                FROM Semesters as S
                WHERE ServiceEvents.DateTimeOccurred <= S.DateEnd
                ORDER BY S.DateEnd
            );");
            AlterColumn("dbo.ServiceEvents", "SemesterId", c => c.Int(nullable: false));
            CreateIndex("dbo.ServiceEvents", "SemesterId");
            AddForeignKey("dbo.ServiceEvents", "SemesterId", "dbo.Semesters", "SemesterId", cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.ServiceEvents", "SemesterId", "dbo.Semesters");
            DropIndex("dbo.ServiceEvents", new[] { "SemesterId" });
            DropColumn("dbo.ServiceEvents", "SemesterId");
        }
    }
}
