namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveSemesterClassTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SemesterClasses", "ClassId", "dbo.Classes");
            DropForeignKey("dbo.SemesterClasses", "SemesterId", "dbo.Semesters");
            DropIndex("dbo.SemesterClasses", "IX_SemesterClass");
            DropTable("dbo.SemesterClasses");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SemesterClasses",
                c => new
                    {
                        ClassId = c.Int(nullable: false),
                        SemesterId = c.Int(nullable: false),
                        SemesterClassId = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.SemesterClassId);
            
            CreateIndex("dbo.SemesterClasses", new[] { "ClassId", "SemesterId" }, unique: true, name: "IX_SemesterClass");
            AddForeignKey("dbo.SemesterClasses", "SemesterId", "dbo.Semesters", "SemesterId", cascadeDelete: true);
            AddForeignKey("dbo.SemesterClasses", "ClassId", "dbo.Classes", "ClassId", cascadeDelete: true);
        }
    }
}
