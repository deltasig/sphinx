namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkOrders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WorkOrderComments",
                c => new
                    {
                        WorkOrderCommentId = c.Int(nullable: false, identity: true),
                        WorkOrderId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        SubmittedOn = c.DateTime(nullable: false),
                        Text = c.String(),
                    })
                .PrimaryKey(t => t.WorkOrderCommentId)
                .ForeignKey("dbo.WorkOrders", t => t.WorkOrderId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.WorkOrderId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.WorkOrders",
                c => new
                    {
                        WorkOrderId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Title = c.String(),
                        Description = c.String(),
                        Result = c.String(),
                    })
                .PrimaryKey(t => t.WorkOrderId)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.WorkOrderPriorityChanges",
                c => new
                    {
                        WorkOrderPriorityChangeId = c.Int(nullable: false, identity: true),
                        WorkOrderId = c.Int(nullable: false),
                        WorkOrderPriorityId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        ChangedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.WorkOrderPriorityChangeId)
                .ForeignKey("dbo.WorkOrderPriorities", t => t.WorkOrderPriorityId, cascadeDelete: true)
                .ForeignKey("dbo.WorkOrders", t => t.WorkOrderId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.WorkOrderId)
                .Index(t => t.WorkOrderPriorityId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.WorkOrderPriorities",
                c => new
                    {
                        WorkOrderPriorityId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.WorkOrderPriorityId);
            
            CreateTable(
                "dbo.WorkOrderStatusChanges",
                c => new
                    {
                        WorkOrderStatusChangeId = c.Int(nullable: false, identity: true),
                        WorkOrderId = c.Int(nullable: false),
                        WorkOrderStatusId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        ChangedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.WorkOrderStatusChangeId)
                .ForeignKey("dbo.WorkOrderStatuses", t => t.WorkOrderStatusId, cascadeDelete: true)
                .ForeignKey("dbo.WorkOrders", t => t.WorkOrderId, cascadeDelete: true)
                .ForeignKey("dbo.Members", t => t.UserId)
                .Index(t => t.WorkOrderId)
                .Index(t => t.WorkOrderStatusId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.WorkOrderStatuses",
                c => new
                    {
                        WorkOrderStatusId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.WorkOrderStatusId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WorkOrderStatusChanges", "UserId", "dbo.Members");
            DropForeignKey("dbo.WorkOrders", "UserId", "dbo.Members");
            DropForeignKey("dbo.WorkOrderPriorityChanges", "UserId", "dbo.Members");
            DropForeignKey("dbo.WorkOrderComments", "UserId", "dbo.Members");
            DropForeignKey("dbo.WorkOrderStatusChanges", "WorkOrderId", "dbo.WorkOrders");
            DropForeignKey("dbo.WorkOrderStatusChanges", "WorkOrderStatusId", "dbo.WorkOrderStatuses");
            DropForeignKey("dbo.WorkOrderPriorityChanges", "WorkOrderId", "dbo.WorkOrders");
            DropForeignKey("dbo.WorkOrderPriorityChanges", "WorkOrderPriorityId", "dbo.WorkOrderPriorities");
            DropForeignKey("dbo.WorkOrderComments", "WorkOrderId", "dbo.WorkOrders");
            DropIndex("dbo.WorkOrderStatusChanges", new[] { "UserId" });
            DropIndex("dbo.WorkOrderStatusChanges", new[] { "WorkOrderStatusId" });
            DropIndex("dbo.WorkOrderStatusChanges", new[] { "WorkOrderId" });
            DropIndex("dbo.WorkOrderPriorityChanges", new[] { "UserId" });
            DropIndex("dbo.WorkOrderPriorityChanges", new[] { "WorkOrderPriorityId" });
            DropIndex("dbo.WorkOrderPriorityChanges", new[] { "WorkOrderId" });
            DropIndex("dbo.WorkOrders", new[] { "UserId" });
            DropIndex("dbo.WorkOrderComments", new[] { "UserId" });
            DropIndex("dbo.WorkOrderComments", new[] { "WorkOrderId" });
            DropTable("dbo.WorkOrderStatuses");
            DropTable("dbo.WorkOrderStatusChanges");
            DropTable("dbo.WorkOrderPriorities");
            DropTable("dbo.WorkOrderPriorityChanges");
            DropTable("dbo.WorkOrders");
            DropTable("dbo.WorkOrderComments");
        }
    }
}
