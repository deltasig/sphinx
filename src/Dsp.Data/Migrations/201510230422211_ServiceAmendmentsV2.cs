namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ServiceAmendmentsV2 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ServiceAmendments", newName: "ServiceEventAmendments");
            CreateTable(
                "dbo.ServiceHourAmendments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        SemesterId = c.Int(nullable: false),
                        AmountHours = c.Double(nullable: false),
                        Reason = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserId)
                .Index(t => t.SemesterId);
            
            AddColumn("dbo.Semesters", "MinimumServiceEvents", c => c.Int(nullable: false));
            AddColumn("dbo.ServiceEventAmendments", "NumberEvents", c => c.Int(nullable: false));
            DropColumn("dbo.ServiceEventAmendments", "AmountHours");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ServiceEventAmendments", "AmountHours", c => c.Double(nullable: false));
            DropIndex("dbo.ServiceHourAmendments", new[] { "SemesterId" });
            DropIndex("dbo.ServiceHourAmendments", new[] { "UserId" });
            DropColumn("dbo.ServiceEventAmendments", "NumberEvents");
            DropColumn("dbo.Semesters", "MinimumServiceEvents");
            DropTable("dbo.ServiceHourAmendments");
            RenameTable(name: "dbo.ServiceEventAmendments", newName: "ServiceAmendments");
        }
    }
}
