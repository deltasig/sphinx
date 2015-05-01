namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkOrderStringLengthCorrection : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.WorkOrderComments", "Text", c => c.String(nullable: false, maxLength: 3000));
            AlterColumn("dbo.WorkOrders", "Title", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.WorkOrders", "Description", c => c.String(nullable: false, maxLength: 3000));
            AlterColumn("dbo.WorkOrders", "Result", c => c.String(maxLength: 3000));
            AlterColumn("dbo.WorkOrderPriorities", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.WorkOrderStatuses", "Name", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.WorkOrderStatuses", "Name", c => c.String());
            AlterColumn("dbo.WorkOrderPriorities", "Name", c => c.String());
            AlterColumn("dbo.WorkOrders", "Result", c => c.String());
            AlterColumn("dbo.WorkOrders", "Description", c => c.String());
            AlterColumn("dbo.WorkOrders", "Title", c => c.String());
            AlterColumn("dbo.WorkOrderComments", "Text", c => c.String());
        }
    }
}
