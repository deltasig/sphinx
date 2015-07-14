namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PositionInquiries : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Positions", "Inquiries", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Positions", "Inquiries");
        }
    }
}
