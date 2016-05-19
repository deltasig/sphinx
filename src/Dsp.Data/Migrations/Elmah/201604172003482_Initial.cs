namespace Dsp.Data.Migrations.Elmah
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ELMAH_Error",
                c => new
                    {
                        ErrorId = c.Guid(nullable: false),
                        Application = c.String(maxLength: 60),
                        Host = c.String(maxLength: 50),
                        Type = c.String(maxLength: 100),
                        Source = c.String(maxLength: 60),
                        Message = c.String(maxLength: 500),
                        User = c.String(maxLength: 50),
                        StatusCode = c.Int(nullable: false),
                        TimeUtc = c.DateTime(nullable: false),
                        Sequence = c.Int(nullable: false),
                        AllXml = c.String(),
                    })
                .PrimaryKey(t => t.ErrorId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ELMAH_Error");
        }
    }
}
