namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class laundryprimarykey : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.LaundrySignup");
            AddPrimaryKey("dbo.LaundrySignup", "DateTimeShift");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.LaundrySignup");
            AddPrimaryKey("dbo.LaundrySignup", new[] { "UserId", "DateTimeShift" });
        }
    }
}
