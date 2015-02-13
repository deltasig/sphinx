namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClassFiles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClassFiles",
                c => new
                    {
                        ClassFileId = c.Int(nullable: false, identity: true),
                        ClassId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        UploadedOn = c.DateTime(nullable: false),
                        LastAccessedOn = c.DateTime(),
                        DownloadCount = c.Int(nullable: false),
                        AwsCode = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.ClassFileId)
                .ForeignKey("dbo.Classes", t => t.ClassId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ClassId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClassFiles", "UserId", "dbo.Members");
            DropForeignKey("dbo.ClassFiles", "ClassId", "dbo.Classes");
            DropIndex("dbo.ClassFiles", new[] { "UserId" });
            DropIndex("dbo.ClassFiles", new[] { "ClassId" });
            DropTable("dbo.ClassFiles");
        }
    }
}
