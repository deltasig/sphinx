namespace DeltaSigmaPhiWebsite.Migrations
{
    using Models;
    using Models.Entities;
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Security;
    using WebMatrix.WebData;

    internal sealed class Configuration : DbMigrationsConfiguration<DspContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(DspContext context)
        {
            if (!WebSecurity.Initialized)
            {
                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "Members", "UserId", "UserName", true);
            }

            context.MemberStatus.AddOrUpdate(m => m.StatusName,
                new MemberStatus { StatusName = "Pledge"},
                new MemberStatus { StatusName = "Neophyte"},
                new MemberStatus { StatusName = "Active"},
                new MemberStatus { StatusName = "Alumnus"},
                new MemberStatus { StatusName = "Affiliate"}
            );

            context.Semesters.AddOrUpdate(s => s.DateStart,
                new Semester { DateStart = new DateTime(2010, 1, 1), DateEnd = new DateTime(2010, 5, 31) },
                new Semester { DateStart = new DateTime(2010, 8, 1), DateEnd = new DateTime(2010, 12, 31) },
                new Semester { DateStart = new DateTime(2011, 1, 1), DateEnd = new DateTime(2011, 5, 31) },
                new Semester { DateStart = new DateTime(2011, 8, 1), DateEnd = new DateTime(2011, 12, 31) },
                new Semester { DateStart = new DateTime(2012, 1, 1), DateEnd = new DateTime(2012, 5, 31) },
                new Semester { DateStart = new DateTime(2012, 8, 1), DateEnd = new DateTime(2012, 12, 31) },
                new Semester { DateStart = new DateTime(2013, 1, 1), DateEnd = new DateTime(2013, 5, 31) },
                new Semester { DateStart = new DateTime(2013, 8, 1), DateEnd = new DateTime(2013, 12, 31) },
                new Semester { DateStart = new DateTime(2014, 1, 1), DateEnd = new DateTime(2014, 6, 30) },
                new Semester { DateStart = new DateTime(2014, 8, 1), DateEnd = new DateTime(2014, 12, 31) }
            );

            context.Departments.AddOrUpdate(d => d.DepartmentName,
                new Department { DepartmentName = "Aerospace & Mechanical Engineering" },
                new Department { DepartmentName = "Arts, Languages, and Philosophy" },
                new Department { DepartmentName = "Biological Sciences" },
                new Department { DepartmentName = "Business" },
                new Department { DepartmentName = "Materials Science & Engineering" }, //5
                new Department { DepartmentName = "Chemical & Biochemical Engineering" },
                new Department { DepartmentName = "Chemistry" },
                new Department { DepartmentName = "Civil, Architectural, and Environmental Engineering" },
                new Department { DepartmentName = "Computer Science" },
                new Department { DepartmentName = "Economics" }, //10
                new Department { DepartmentName = "Education" },
                new Department { DepartmentName = "Electrical & Computer Engineering" },
                new Department { DepartmentName = "Engineering Management & Systems Engineering" },
                new Department { DepartmentName = "English & Technical Communication" },
                new Department { DepartmentName = "Geosciences & Geological & Petroleum Engineering" }, //15
                new Department { DepartmentName = "Information Science & Technology" },
                new Department { DepartmentName = "Mathematics & Statistics" },
                new Department { DepartmentName = "Mining Engineering" },
                new Department { DepartmentName = "Nuclear Engineering" },
                new Department { DepartmentName = "Physics" }, //20
                new Department { DepartmentName = "Psychology" }
            );

            context.SaveChanges();

            context.Majors.AddOrUpdate(m => m.MajorName,
                new Major { DepartmentId = 1, MajorName = "Aerospace Engineering" },
                new Major { DepartmentId = 1, MajorName = "Mechanical Engineering" },
                new Major { DepartmentId = 2, MajorName = "Art" },
                new Major { DepartmentId = 2, MajorName = "Philosophy" },
                new Major { DepartmentId = 3, MajorName = "Biology" },
                new Major { DepartmentId = 4, MajorName = "Business" },
                new Major { DepartmentId = 5, MajorName = "Ceramic Engineering" },
                new Major { DepartmentId = 5, MajorName = "Metallurgical Engineering" },
                new Major { DepartmentId = 6, MajorName = "Chemical Engineering" },
                new Major { DepartmentId = 7, MajorName = "Chemistry" },
                new Major { DepartmentId = 8, MajorName = "Civil Engineering" },
                new Major { DepartmentId = 8, MajorName = "Architectural Engineering" },
                new Major { DepartmentId = 8, MajorName = "Environmental Engineering" },
                new Major { DepartmentId = 9, MajorName = "Computer Science" },
                new Major { DepartmentId = 10, MajorName = "Economics" },
                new Major { DepartmentId = 11, MajorName = "Education" },
                new Major { DepartmentId = 12, MajorName = "Electrical Engineering" },
                new Major { DepartmentId = 12, MajorName = "Computer Engineering" },
                new Major { DepartmentId = 13, MajorName = "Engineering Management" },
                new Major { DepartmentId = 14, MajorName = "English" },
                new Major { DepartmentId = 14, MajorName = "Technical Communication" },
                new Major { DepartmentId = 15, MajorName = "Geological Engineering" },
                new Major { DepartmentId = 15, MajorName = "Geology" },
                new Major { DepartmentId = 15, MajorName = "Petroleum Engineering" },
                new Major { DepartmentId = 16, MajorName = "Information Science & Technology" },
                new Major { DepartmentId = 17, MajorName = "Mathematics" },
                new Major { DepartmentId = 18, MajorName = "Mining Engineering" },
                new Major { DepartmentId = 19, MajorName = "Nuclear Engineering" },
                new Major { DepartmentId = 20, MajorName = "Physics" },
                new Major { DepartmentId = 21, MajorName = "Psychology" }
            );

            context.SaveChanges();

            context.Positions.AddOrUpdate(m => m.PositionName,
                new Position { IsElected = true, IsExecutive = true, PositionName = "Administrator" },
                new Position { IsElected = true, IsExecutive = true, PositionName = "President" },
                new Position { IsElected = true, IsExecutive = true, PositionName = "Vice President External" },
                new Position { IsElected = true, IsExecutive = true, PositionName = "Vice President Internal" },
                new Position { IsElected = true, IsExecutive = true, PositionName = "Treasurer", },
                new Position { IsElected = true, IsExecutive = true, PositionName = "Secretary" },
                new Position { IsElected = true, IsExecutive = true, PositionName = "Sergeant-at-Arms" },
                new Position { IsElected = true, IsExecutive = true, PositionName = "Executive Board Representative" },
                new Position { IsElected = false, IsExecutive = true, PositionName = "Academics" },
                new Position { IsElected = false, IsExecutive = true, PositionName = "Brotherhood" },
                new Position { IsElected = false, IsExecutive = true, PositionName = "Director of Recruitment" },
                new Position { IsElected = false, IsExecutive = true, PositionName = "House Manager" },
                new Position { IsElected = false, IsExecutive = true, PositionName = "New Member Education" },
                new Position { IsElected = false, IsExecutive = true, PositionName = "Social" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Alumni Relations" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Community Service" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Greek Week" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "House Steward" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Philanthropy" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Public Relations" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "St. Pat's" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "IFC" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "StuCo" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Recruitment Coordinator" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Chapter Advisor" }
            );

            context.SaveChanges();

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
