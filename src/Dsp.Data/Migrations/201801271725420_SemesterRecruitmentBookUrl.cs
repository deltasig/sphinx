namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SemesterRecruitmentBookUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Semesters", "RecruitmentBookUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Semesters", "RecruitmentBookUrl");
        }
    }
}
