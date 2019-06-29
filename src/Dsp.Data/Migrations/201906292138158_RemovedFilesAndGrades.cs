namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedFilesAndGrades : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ClassFiles", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.ClassFiles", "UserId", "dbo.Members");
            DropIndex("dbo.ClassFiles", new[] { "ClassId" });
            DropIndex("dbo.ClassFiles", new[] { "UserId" });
            DropColumn("dbo.ClassesTaken", "MidtermGrade");
            DropColumn("dbo.ClassesTaken", "FinalGrade");
            DropTable("dbo.ClassFiles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ClassFiles",
                c => new
                    {
                        ClassFileId = c.Int(nullable: false, identity: true),
                        ClassId = c.Int(nullable: false),
                        UserId = c.Int(),
                        UploadedOn = c.DateTime(nullable: false),
                        LastAccessedOn = c.DateTime(),
                        DownloadCount = c.Int(nullable: false),
                        AwsCode = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.ClassFileId);
            
            AddColumn("dbo.ClassesTaken", "FinalGrade", c => c.String(maxLength: 1));
            AddColumn("dbo.ClassesTaken", "MidtermGrade", c => c.String(maxLength: 1));
            CreateIndex("dbo.ClassFiles", "UserId");
            CreateIndex("dbo.ClassFiles", "ClassId");
            AddForeignKey("dbo.ClassFiles", "UserId", "dbo.Members", "UserId");
            AddForeignKey("dbo.ClassFiles", "ClassId", "dbo.Classes", "ClassId", cascadeDelete: true);
        }
    }
}
