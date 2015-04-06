namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SoberTypeStage1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SoberTypes",
                c => new
                    {
                        SoberTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.SoberTypeId);
            
            AddColumn("dbo.SoberSignups", "SoberTypeId", c => c.Int(nullable: false, defaultValue:0));
            AddColumn("dbo.SoberSignups", "Description", c => c.String(maxLength: 100));
            Sql("UPDATE [dbo].[SoberSignups] SET SoberTypeId = Type + 1");
            Sql("INSERT INTO [dbo].[SoberTypes] VALUES ('Driver')");
            Sql("INSERT INTO [dbo].[SoberTypes] VALUES ('Officer')");
            AlterColumn("dbo.ScholarshipAnswers", "AnswerText", c => c.String(maxLength: 3000));
            CreateIndex("dbo.SoberSignups", "SoberTypeId");
            AddForeignKey("dbo.SoberSignups", "SoberTypeId", "dbo.SoberTypes", "SoberTypeId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SoberSignups", "SoberTypeId", "dbo.SoberTypes");
            DropIndex("dbo.SoberSignups", new[] { "SoberTypeId" });
            AlterColumn("dbo.ScholarshipAnswers", "AnswerText", c => c.String(nullable: false, maxLength: 3000));
            DropColumn("dbo.SoberSignups", "Description");
            DropColumn("dbo.SoberSignups", "SoberTypeId");
            DropTable("dbo.SoberTypes");
        }
    }
}
