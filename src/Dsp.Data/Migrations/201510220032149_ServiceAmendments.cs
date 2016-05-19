namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServiceAmendments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ServiceAmendments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        SemesterId = c.Int(nullable: false),
                        AmountHours = c.Double(nullable: false),
                        Reason = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Semesters", t => t.SemesterId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.SemesterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ServiceAmendments", "UserId", "dbo.Members");
            DropForeignKey("dbo.ServiceAmendments", "SemesterId", "dbo.Semesters");
            DropIndex("dbo.ServiceAmendments", new[] { "SemesterId" });
            DropIndex("dbo.ServiceAmendments", new[] { "UserId" });
            DropTable("dbo.ServiceAmendments");
        }
    }
}
