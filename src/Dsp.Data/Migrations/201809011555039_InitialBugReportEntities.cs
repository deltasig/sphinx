namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialBugReportEntities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BugReports",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Description = c.String(maxLength: 2500),
                        UrlWithProblem = c.String(maxLength: 100),
                        Response = c.String(maxLength: 2500),
                        IsFixed = c.Boolean(nullable: false),
                        ErroredOn = c.DateTime(nullable: false),
                        ReportedOn = c.DateTime(nullable: false),
                        LastUpdatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.BugImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BugReportId = c.Int(nullable: false),
                        Image = c.Binary(storeType: "image"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BugReports", t => t.BugReportId, cascadeDelete: true)
                .Index(t => t.BugReportId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BugReports", "UserId", "dbo.Members");
            DropForeignKey("dbo.BugImages", "BugReportId", "dbo.BugReports");
            DropIndex("dbo.BugImages", new[] { "BugReportId" });
            DropIndex("dbo.BugReports", new[] { "UserId" });
            DropTable("dbo.BugImages");
            DropTable("dbo.BugReports");
        }
    }
}
