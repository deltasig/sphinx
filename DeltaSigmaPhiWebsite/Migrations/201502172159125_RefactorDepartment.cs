namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RefactorDepartment : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Departments", "DepartmentName", "Name");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Departments", "Name", "DepartmentName");
        }
    }
}
