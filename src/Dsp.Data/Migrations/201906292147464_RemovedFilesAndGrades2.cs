namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedFilesAndGrades2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ClassesTaken", "Dropped");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ClassesTaken", "Dropped", c => c.Boolean());
        }
    }
}
