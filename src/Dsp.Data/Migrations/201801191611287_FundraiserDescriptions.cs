namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FundraiserDescriptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Fundraisers", "Description", c => c.String(nullable: false, maxLength: 2000));
            AddColumn("dbo.Fundraisers", "DonationInstructions", c => c.String(nullable: false, maxLength: 2000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Fundraisers", "DonationInstructions");
            DropColumn("dbo.Fundraisers", "Description");
        }
    }
}
