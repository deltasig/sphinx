namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Emails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Emails",
                c => new
                    {
                        EmailId = c.Int(nullable: false, identity: true),
                        EmailTypeId = c.Int(nullable: false),
                        SentOn = c.DateTime(nullable: false),
                        Destination = c.String(),
                        Body = c.String(),
                    })
                .PrimaryKey(t => t.EmailId)
                .ForeignKey("dbo.EmailTypes", t => t.EmailTypeId, cascadeDelete: true)
                .Index(t => t.EmailTypeId);
            
            CreateTable(
                "dbo.EmailTypes",
                c => new
                    {
                        EmailTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.EmailTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Emails", "EmailTypeId", "dbo.EmailTypes");
            DropIndex("dbo.Emails", new[] { "EmailTypeId" });
            DropTable("dbo.EmailTypes");
            DropTable("dbo.Emails");
        }
    }
}
