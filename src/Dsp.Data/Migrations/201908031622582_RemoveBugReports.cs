namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveBugReports : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BugImages", "BugReportId", "dbo.BugReports");
            DropForeignKey("dbo.BugReports", "UserId", "dbo.Members");
            DropIndex("dbo.BugReports", new[] { "UserId" });
            DropIndex("dbo.BugImages", new[] { "BugReportId" });
            DropTable("dbo.BugReports");
            DropTable("dbo.BugImages");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.BugImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BugReportId = c.Int(nullable: false),
                        Image = c.Binary(storeType: "image"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BugReports",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Description = c.String(nullable: false, maxLength: 2500),
                        UrlWithProblem = c.String(nullable: false, maxLength: 100),
                        Response = c.String(maxLength: 2500),
                        IsFixed = c.Boolean(nullable: false),
                        ReportedOn = c.DateTime(nullable: false),
                        LastUpdatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.BugImages", "BugReportId");
            CreateIndex("dbo.BugReports", "UserId");
            AddForeignKey("dbo.BugReports", "UserId", "dbo.Members", "UserId", cascadeDelete: true);
            AddForeignKey("dbo.BugImages", "BugReportId", "dbo.BugReports", "Id", cascadeDelete: true);
        }
    }
}
