namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncSoberTypeDescLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SoberTypes", "Description", c => c.String(maxLength: 3000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SoberTypes", "Description", c => c.String(maxLength: 2000));
        }
    }
}
