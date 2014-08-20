namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Security;
    using Models;
    using Models.Entities;
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
                new MemberStatus { StatusName = "Affiliate" },
                new MemberStatus { StatusName = "Released" }
            );

            #region Semesters/Pledge Classes

            //const int startDay = 1;
            //const int endDay = 31;
            //const int springStartMonth = 1;
            //const int springEndMonth = 5;
            //const int fallStartMonth = 8;
            //const int fallEndMonth = 12;

            //context.Semesters.AddOrUpdate(s => s.DateStart,
            //    new Semester { DateStart = new DateTime(2006, springStartMonth, startDay), DateEnd = new DateTime(2006, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2006, fallStartMonth, startDay), DateEnd = new DateTime(2006,fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2007, springStartMonth, startDay), DateEnd = new DateTime(2007, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2007, fallStartMonth, startDay), DateEnd = new DateTime(2007,fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2008, springStartMonth, startDay), DateEnd = new DateTime(2008, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2008, fallStartMonth, startDay), DateEnd = new DateTime(2008,fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2009, springStartMonth, startDay), DateEnd = new DateTime(2009, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2009, fallStartMonth, startDay), DateEnd = new DateTime(2009,fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2010, springStartMonth, startDay), DateEnd = new DateTime(2010, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2010, fallStartMonth, startDay), DateEnd = new DateTime(2010,fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2011, springStartMonth, startDay), DateEnd = new DateTime(2011, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2011, fallStartMonth, startDay), DateEnd = new DateTime(2011,fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2012, springStartMonth, startDay), DateEnd = new DateTime(2012, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2012, fallStartMonth, startDay), DateEnd = new DateTime(2012,fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2013, springStartMonth, startDay), DateEnd = new DateTime(2013, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2013, fallStartMonth, startDay), DateEnd = new DateTime(2013,fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2014, springStartMonth, startDay), DateEnd = new DateTime(2014, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2014, fallStartMonth, startDay), DateEnd = new DateTime(2014, fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2015, springStartMonth, startDay), DateEnd = new DateTime(2015, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2015, fallStartMonth, startDay), DateEnd = new DateTime(2015, fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2016, springStartMonth, startDay), DateEnd = new DateTime(2016, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2016, fallStartMonth, startDay), DateEnd = new DateTime(2016, fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2017, springStartMonth, startDay), DateEnd = new DateTime(2017, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2017, fallStartMonth, startDay), DateEnd = new DateTime(2017, fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2018, springStartMonth, startDay), DateEnd = new DateTime(2018, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2018, fallStartMonth, startDay), DateEnd = new DateTime(2018, fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2019, springStartMonth, startDay), DateEnd = new DateTime(2019, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2019, fallStartMonth, startDay), DateEnd = new DateTime(2019, fallEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2020, springStartMonth, startDay), DateEnd = new DateTime(2020, springEndMonth, endDay) },
            //    new Semester { DateStart = new DateTime(2020, fallStartMonth, startDay), DateEnd = new DateTime(2020, fallEndMonth, endDay) }
            //);

            //context.SaveChanges();
            
            //context.PledgeClasses.AddOrUpdate(m => m.PledgeClassName,
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2006, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Alpha 1",
            //        PinningDate = new DateTime(2006, 2, 19)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2006, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Alpha 2",
            //        PinningDate = new DateTime(2006, 2, 22)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2006, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Alpha 3",
            //        PinningDate = new DateTime(2006, 3, 6)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2006, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Beta",
            //        PinningDate = new DateTime(2006, 9, 1)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2007, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Gamma",
            //        PinningDate = new DateTime(2007, 2, 1)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2007, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Delta",
            //        PinningDate = new DateTime(2007, 9, 1)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2008, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Epsilon",
            //        PinningDate = new DateTime(2008, 2, 1)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2008, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Zeta",
            //        PinningDate = new DateTime(2008, 9, 1)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2009, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Eta",
            //        PinningDate = new DateTime(2009, 2, 1)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2009, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Theta",
            //        PinningDate = new DateTime(2009, 9, 1)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2010, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Iota",
            //        PinningDate = new DateTime(2010, 2, 1)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2010, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Kappa",
            //        PinningDate = new DateTime(2010, 9, 19)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2011, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Lambda",
            //        PinningDate = new DateTime(2011, 2, 20)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2011, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Mu",
            //        PinningDate = new DateTime(2011, 9, 18)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2012, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Nu",
            //        PinningDate = new DateTime(2012, 2, 17)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2012, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Xi",
            //        PinningDate = new DateTime(2012, 9, 16)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2013, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Omicron",
            //        PinningDate = new DateTime(2013, 2, 24)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2013, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Pi",
            //        PinningDate = new DateTime(2013, 9, 15)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2014, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Rho",
            //        PinningDate = new DateTime(2014, 2, 16)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2014, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Sigma",
            //        PinningDate = new DateTime(2014, 9, 15)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2015, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Tau",
            //        PinningDate = new DateTime(2015, 2, 16)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2015, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Upsilon",
            //        PinningDate = new DateTime(2015, 9, 15)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2016, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Phi",
            //        PinningDate = new DateTime(2016, 2, 16)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2016, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Chi",
            //        PinningDate = new DateTime(2016, 9, 15)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2017, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Psi",
            //        PinningDate = new DateTime(2017, 2, 16)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2017, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Omega",
            //        PinningDate = new DateTime(2017, 9, 15)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2018, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Alpha Alpha",
            //        PinningDate = new DateTime(2018, 2, 16)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2018, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Alpha Beta",
            //        PinningDate = new DateTime(2018, 9, 15)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2019, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Alpha Gamma",
            //        PinningDate = new DateTime(2019, 2, 16)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2019, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Alpha Delta",
            //        PinningDate = new DateTime(2019, 9, 15)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2020, springStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Alpha Epsilon",
            //        PinningDate = new DateTime(2020, 2, 16)
            //    },
            //    new PledgeClass
            //    {
            //        SemesterId = context.Semesters.Single(s => s.DateStart == new DateTime(2020, fallStartMonth, startDay)).SemesterId,
            //        PledgeClassName = "Alpha Zeta",
            //        PinningDate = new DateTime(2020, 9, 15)
            //    }
            //);

            #endregion

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
                new Position { IsElected = true, IsExecutive = true, PositionName = "Executive Board Representative 1" },
                new Position { IsElected = true, IsExecutive = true, PositionName = "Executive Board Representative 2" },
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
                new Position { IsElected = false, IsExecutive = false, PositionName = "Recruitment Coordinator 1" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Recruitment Coordinator 2" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Recruitment Coordinator 3" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Recruitment Coordinator 4" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Recruitment Coordinator 5" },
                new Position { IsElected = false, IsExecutive = false, PositionName = "Chapter Advisor" }
            );

            context.SaveChanges();

            if (!WebSecurity.UserExists("tjm6f4"))
                WebSecurity.CreateUserAndAccount("tjm6f4", "***REMOVED***", new
                {
                    FirstName = "Tyler", 
                    LastName = "Morrow", 
                    Email = "tjm6f4@mst.edu", 
                    StatusId = context.MemberStatus.Single(s => s.StatusName == "Active").StatusId,
                    ExpectedGraduationId = 9,
                    PledgeClassId = 12,
                    RequiredStudyHours = 0
                });
            var userId = WebSecurity.GetUserId("tjm6f4");
            if (!context.Addresses.Select(a => a.UserId == userId).Any())
            {
                context.Addresses.AddOrUpdate(
                    new Address { UserId = userId, Type = "Mailing" },
                    new Address { UserId = userId, Type = "Permanent" });
                context.SaveChanges();
            }
            if (!context.PhoneNumbers.Select(a => a.UserId == userId).Any())
            {
                context.PhoneNumbers.AddOrUpdate(m => m.Type,
                    new PhoneNumber { UserId = userId, Type = "Mobile" },
                    new PhoneNumber { UserId = userId, Type = "Emergency Contact" });
                context.SaveChanges();
            }

            if (!Roles.GetRolesForUser("tjm6f4").Contains("Administrator"))
                Roles.AddUsersToRoles(new[] { "tjm6f4" }, new[] { "Administrator" });
        }
    }
}
