namespace Dsp.Data.Migrations
{
    using Dsp.Data;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<SphinxDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(SphinxDbContext context)
        {
            /*
            #region Member Statuses
            context.MemberStatus.AddOrUpdate(m => m.StatusName,
                new MemberStatus { StatusName = "New" },
                new MemberStatus { StatusName = "Neophyte" },
                new MemberStatus { StatusName = "Active" },
                new MemberStatus { StatusName = "Alumnus" },
                new MemberStatus { StatusName = "Affiliate" },
                new MemberStatus { StatusName = "Released" }
            );
            context.SaveChanges();

            #endregion

            #region Work Order Statuses

            context.WorkOrderStatuses.AddOrUpdate(m => m.Name,
                new WorkOrderStatus { Name = "Unread" },
                new WorkOrderStatus { Name = "Under Review" },
                new WorkOrderStatus { Name = "In Progress" },
                new WorkOrderStatus { Name = "On Hold" },
                new WorkOrderStatus { Name = "Closed" }
            );
            context.SaveChanges();

            #endregion

            #region Work Order Priorities

            context.WorkOrderPriorities.AddOrUpdate(m => m.Name,
                new WorkOrderPriority { Name = "Low" },
                new WorkOrderPriority { Name = "Moderate" },
                new WorkOrderPriority { Name = "High" }
            );
            context.SaveChanges();

            #endregion

            #region Semesters

            const int startYear = 2006;
            const int endYear = 2020;
            const int springStartMonth = 1;
            const int springEndMonth = 5;
            const int fallStartMonth = 8;
            const int fallEndMonth = 12;
            const int startDay = 20;
            const int endDay = 15;

            var semesters = new List<Semester>();
            for (var y = startYear; y <= endYear; y++)
            {
                semesters.Add(new Semester
                {
                    DateStart = new DateTime(y, springStartMonth, startDay),
                    DateEnd = new DateTime(y, springEndMonth, endDay),
                    TransitionDate = new DateTime(y, springEndMonth, endDay)
                });
                semesters.Add(new Semester
                {
                    DateStart = new DateTime(y, fallStartMonth, startDay),
                    DateEnd = new DateTime(y, fallEndMonth, endDay),
                    TransitionDate = new DateTime(y, fallEndMonth, endDay)
                });
            }
            context.Semesters.AddOrUpdate(s => s.DateStart, semesters.OrderBy(s => s.DateStart).ToArray());
            context.SaveChanges();

            #endregion

            #region Pledge Classes

            var pledgeClassNames = new List<string>
            { 
                // 10 per row
                "Alpha 1", "Alpha 2", "Alpha 3", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta",
                "Iota", "Kappa", "Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi", "Rho", "Sigma",
                "Tau", "Upsilon", "Phi", "Chi", "Psi", "Omega", "Alpha Alpha", "Alpha Beta", "Alpha Gamma", "Alpha Delta",
                "Alpha Epsilon", "Alpha Zeta"
            };

            var pledgeClasses = new List<PledgeClass>
            {
                new PledgeClass
                {
                    SemesterId = semesters.Single(s => s.DateStart == new DateTime(2006, springStartMonth, startDay)).SemesterId,
                    PledgeClassName = pledgeClassNames[0],
                    PinningDate = new DateTime(2006, 2, 19)
                },
                new PledgeClass
                {
                    SemesterId = semesters.Single(s => s.DateStart == new DateTime(2006, springStartMonth, startDay)).SemesterId,
                    PledgeClassName = pledgeClassNames[1],
                    PinningDate = new DateTime(2006, 3, 19)
                },
                new PledgeClass
                {
                    SemesterId = semesters.Single(s => s.DateStart == new DateTime(2006, springStartMonth, startDay)).SemesterId,
                    PledgeClassName = pledgeClassNames[2],
                    PinningDate = new DateTime(2006, 4, 19)
                },
                new PledgeClass
                {
                    SemesterId = semesters.Single(s => s.DateStart == new DateTime(2006, fallStartMonth, startDay)).SemesterId,
                    PledgeClassName = pledgeClassNames[3],
                    PinningDate = (new DateTime(2006, fallStartMonth, startDay)).AddDays(28),
                    InitiationDate = (new DateTime(2006, fallStartMonth, startDay)).AddDays(91)
                }
            };
            var i = 4;
            for (var y = startYear + 1; y <= endYear; y++)
            {
                pledgeClasses.Add(new PledgeClass
                {
                    SemesterId = semesters.Single(s => s.DateStart == new DateTime(y, springStartMonth, startDay)).SemesterId,
                    PledgeClassName = pledgeClassNames[i],
                    PinningDate = (new DateTime(y, springStartMonth, startDay)).AddDays(28),
                    InitiationDate = (new DateTime(y, springStartMonth, startDay)).AddDays(91)
                });
                i++;
                pledgeClasses.Add(new PledgeClass
                {
                    SemesterId = semesters.Single(s => s.DateStart == new DateTime(y, fallStartMonth, startDay)).SemesterId,
                    PledgeClassName = pledgeClassNames[i],
                    PinningDate = (new DateTime(y, fallStartMonth, startDay)).AddDays(28),
                    InitiationDate = (new DateTime(y, fallStartMonth, startDay)).AddDays(91)
                });
                i++;
            }

            context.PledgeClasses.AddOrUpdate(s => s.PledgeClassName, pledgeClasses.OrderBy(s => s.PinningDate).ToArray());
            context.SaveChanges();

            #endregion

            #region Positions

            context.Roles.AddOrUpdate(m => m.Name,
                new Position { IsElected = true, IsExecutive = true, Type = Position.PositionType.Active, Name = "Administrator" },
                new Position { IsElected = true, IsExecutive = true, Type = Position.PositionType.Active, Name = "President" },
                new Position { IsElected = true, IsExecutive = true, Type = Position.PositionType.Active, Name = "Vice President External" },
                new Position { IsElected = true, IsExecutive = true, Type = Position.PositionType.Active, Name = "Vice President Internal" },
                new Position { IsElected = true, IsExecutive = true, Type = Position.PositionType.Active, Name = "Treasurer", },
                new Position { IsElected = true, IsExecutive = true, Type = Position.PositionType.Active, Name = "Secretary" },
                new Position { IsElected = true, IsExecutive = true, Type = Position.PositionType.Active, Name = "Sergeant-at-Arms" },
                new Position { IsElected = true, IsExecutive = true, Type = Position.PositionType.Active, Name = "Executive Board Representative 1" },
                new Position { IsElected = true, IsExecutive = true, Type = Position.PositionType.Active, Name = "Executive Board Representative 2" },
                new Position { IsElected = false, IsExecutive = true, Type = Position.PositionType.Active, Name = "Academics" },
                new Position { IsElected = false, IsExecutive = true, Type = Position.PositionType.Active, Name = "Brotherhood" },
                new Position { IsElected = false, IsExecutive = true, Type = Position.PositionType.Active, Name = "Director of Recruitment" },
                new Position { IsElected = false, IsExecutive = true, Type = Position.PositionType.Active, Name = "House Manager" },
                new Position { IsElected = false, IsExecutive = true, Type = Position.PositionType.Active, Name = "New Member Education" },
                new Position { IsElected = false, IsExecutive = true, Type = Position.PositionType.Active, Name = "Social" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Alumni Relations" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Service" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Greek Week" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "House Steward" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Philanthropy" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Public Relations" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "St. Pat's" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "IFC" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "StuCo Representative I" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Recruitment Coordinator I" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Recruitment Coordinator II" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Recruitment Coordinator III" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Recruitment Coordinator IV" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Recruitment Coordinator V" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Recruitment Coordinator VI" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Alumni, Name = "Chapter Advisor" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Vice President Growth" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Vice President Operations" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "Vice President Programming" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Alumni, Name = "Growth Advisor" },
                new Position { IsElected = false, IsExecutive = false, Type = Position.PositionType.Active, Name = "StuCo Representative II" }
            );
            context.SaveChanges();

            #endregion

            #region Admin Account

            var userStore = new SphinxUserStore(context);
            var userManager = new ApplicationUserManager(userStore);
            if (!(context.Users.Any(u => u.UserName == "admin")))
            {
                var userToInsert = new Member
                {
                    UserName = "admin", 
                    Email = "admin@mst.edu",
                    FirstName = "Admin",
                    LastName = "User",
                    StatusId = 4,
                    PledgeClassId = 12,
                    ExpectedGraduationId = 19
                    ShirtSize = "M",
                };
                userManager.Create(userToInsert, "password");
            }

            var user = userManager.FindByName("admin");

            if (!userManager.GetRoles(user.Id).Contains("Administrator"))
            {
                var adminPosition = context.Roles.Single(r => r.Name == "Administrator");
                ((DspRoleProvider)Roles.Provider).AddUserToRole(user.Id, adminPosition.Id, 1);
            }

            #endregion
            */
        }
    }
}
