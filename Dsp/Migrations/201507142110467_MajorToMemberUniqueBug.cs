namespace Dsp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MajorToMemberUniqueBug : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.MajorToMembers", "IX_MajorToMember");
            CreateIndex("dbo.MajorToMembers", new[] { "MajorId", "UserId", "DegreeLevel" }, unique: true, name: "IX_MajorToMember");
        }
        
        public override void Down()
        {
            DropIndex("dbo.MajorToMembers", "IX_MajorToMember");
            CreateIndex("dbo.MajorToMembers", new[] { "MajorId", "UserId" }, unique: true, name: "IX_MajorToMember");
        }
    }
}
