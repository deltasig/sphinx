namespace DeltaSigmaPhiWebsite.Migrations
{
    using Models;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Security;
    using WebMatrix.WebData;

    internal sealed class Configuration : DbMigrationsConfiguration<DspContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(DspContext context)
        {
            //  This method will be called after migrating to the latest version.

            WebSecurity.InitializeDatabaseConnection("DspContext", "Members", "UserId", "UserName", true);
            
            context.MemberStatus.Add(new MemberStatus { StatusName = "Pledge" });
            context.MemberStatus.Add(new MemberStatus { StatusName = "Neophyte" });
            context.MemberStatus.Add(new MemberStatus { StatusName = "Active" });
            context.MemberStatus.Add(new MemberStatus { StatusName = "Alumnus" });
            context.MemberStatus.Add(new MemberStatus { StatusName = "Affiliate" });

            context.SaveChanges();

            if (!Roles.RoleExists("Administrator"))
                Roles.CreateRole("Administrator");

            if (!WebSecurity.UserExists("tjm6f4"))
                WebSecurity.CreateUserAndAccount("tjm6f4", "ultima0", new
                {
                    FirstName = "Tyler", 
                    LastName = "Morrow", 
                    NickName = "Ty", 
                    Email = "tjm6f4@mst.edu", 
                    StatusId = context.MemberStatus.Single(s => s.StatusName == "Alumnus").StatusId
                });

            if (!Roles.GetRolesForUser("tjm6f4").Contains("Administrator"))
                Roles.AddUsersToRoles(new[] { "tjm6f4" }, new[] { "Administrator" });
        }
    }
}
