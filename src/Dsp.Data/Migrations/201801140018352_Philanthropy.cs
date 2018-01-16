namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Philanthropy : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Donations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FundraiserId = c.Int(nullable: false),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 50),
                        Email = c.String(),
                        PhoneNumber = c.String(),
                        Amount = c.Double(nullable: false),
                        ReceivedOn = c.DateTime(),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Fundraisers", t => t.FundraiserId, cascadeDelete: true)
                .Index(t => t.FundraiserId);
            
            CreateTable(
                "dbo.Fundraisers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PhilanthropyId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 150),
                        Goal = c.Double(nullable: false),
                        BeginsOn = c.DateTime(),
                        EndsOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Philanthropies", t => t.PhilanthropyId, cascadeDelete: true)
                .Index(t => t.PhilanthropyId);
            
            CreateTable(
                "dbo.Philanthropies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        Description = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Fundraisers", "PhilanthropyId", "dbo.Philanthropies");
            DropForeignKey("dbo.Donations", "FundraiserId", "dbo.Fundraisers");
            DropIndex("dbo.Fundraisers", new[] { "PhilanthropyId" });
            DropIndex("dbo.Donations", new[] { "FundraiserId" });
            DropTable("dbo.Philanthropies");
            DropTable("dbo.Fundraisers");
            DropTable("dbo.Donations");
        }
    }
}
