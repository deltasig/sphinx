namespace Dsp.Web.Migrations.Elmah
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullableSequence : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ELMAH_Error", "Sequence", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ELMAH_Error", "Sequence", c => c.Int(nullable: false));
        }
    }
}
