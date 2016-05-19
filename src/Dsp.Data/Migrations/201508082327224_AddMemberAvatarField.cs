namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMemberAvatarField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Members", "AvatarPath", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Members", "AvatarPath");
        }
    }
}
