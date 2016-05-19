namespace Dsp.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncreasedClassShorthand : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Classes", "CourseShorthand", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Classes", "CourseShorthand", c => c.String(nullable: false, maxLength: 15));
        }
    }
}
