namespace Dsp.Web.Extensions
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web.Security;

    public sealed class DspRoleProvider : RoleProvider
    {
        private string _applicationName;

        public override string ApplicationName
        {
            get
            {
                return _applicationName;
            }
            set
            {
                _applicationName = value;
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (name.Length == 0)
            {
                name = "CustomRoleProvider";
            }

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Custom Role Provider");
            }

            //Initialize the abstract base class.
            base.Initialize(name, config);

            _applicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
        }

        public override void AddUsersToRoles(string[] userNames, string[] positionNames)
        {
            try
            {
                using (var db = new SphinxDbContext())
                {
                    foreach (var userName in userNames)
                    {
                        // find each user in users table
                        var name = userName;
                        var user = (from m in db.Users
                                    where m.UserName == name
                                    select m).FirstOrDefault();

                        if (user == null) continue;
                        // find all roles that are contained in the roleNames
                        var allPositions = (from p in db.Roles select p).ToList();

                        var positionIds = new List<int>();

                        foreach (var position in allPositions)
                        {
                            positionIds.AddRange(from positionName in positionNames
                                                 where position.Name == positionName
                                                 select position.Id);
                        }

                        if (positionIds.Count <= 0) continue;

                        foreach (var positionId in positionIds)
                        {
                            var memberInRole = (from mip in db.Leaders
                                                where mip.Member.Id == user.Id &&
                                                      mip.RoleId == positionId
                                                select mip).FirstOrDefault();
                            if (memberInRole == null)
                            {
                                memberInRole = new Leader
                                {
                                    UserId = user.Id,
                                    RoleId = positionId,
                                    SemesterId = (from s in db.Semesters
                                                  orderby s.DateEnd descending
                                                  select s.SemesterId).First(),
                                    AppointedOn = DateTime.UtcNow
                                };
                                db.Leaders.Add(memberInRole);
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public async void AddUserToRole(int userId, int positionId, int semesterId)
        {
            try
            {
                using (var db = new SphinxDbContext())
                {
                    db.Leaders.Add(new Leader
                    {
                        UserId = userId,
                        RoleId = positionId,
                        SemesterId = semesterId,
                        AppointedOn = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
            }
            catch
            {
            }
        }

        public override void CreateRole(string roleName)
        {
            try
            {
                using (var db = new SphinxDbContext())
                {
                    db.Roles.Add(new Position
                    {
                        Name = roleName
                    });
                    db.SaveChanges();
                }
            }
            catch
            {
            }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            var ret = false;

            using (var db = new SphinxDbContext())
            {
                try
                {
                    var position = (from pos in db.Roles
                                    where pos.Name == roleName
                                    select pos).SingleOrDefault();

                    if (position != null)
                    {
                        db.Roles.Remove(position);
                        db.SaveChanges();

                        ret = true;
                    }
                }
                catch
                {
                    ret = false;
                }
            }

            return ret;
        }

        public override string[] FindUsersInRole(string positionName, string usernameToMatch)
        {
            var users = new List<string>();

            using (var db = new SphinxDbContext())
            {
                try
                {
                    var usersInRole = from mip in db.Leaders
                                      where mip.Position.Name == positionName &&
                                            mip.Member.UserName == usernameToMatch
                                      select mip;

                    if (usersInRole.Any())
                    {
                        users.AddRange(usersInRole.Select(userInRole => userInRole.Member.UserName));
                    }
                }
                catch
                {
                }
            }

            return users.ToArray();
        }

        public override string[] GetAllRoles()
        {
            var roles = new List<string>();

            using (var db = new SphinxDbContext())
            {
                try
                {
                    var positions = from r in db.Roles
                                    select r;

                    roles.AddRange(positions.Select(p => p.Name));
                }
                catch
                {
                }
            }

            return roles.ToArray();
        }

        public override string[] GetRolesForUser(string userName)
        {
            var roles = new List<string>();

            using (var db = new SphinxDbContext())
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var member = db.Users.Single(m => m.UserName == userName);
                    var positions = member
                        .PositionsHeld
                        .Where(m =>
                            (m.Member.UserName == userName &&
                            m.Semester.TransitionDate > now) ||
                            m.Position.Name == "Administrator")
                        .Select(m => m.Position.Name)
                        .ToList();
                    if (positions.Contains("Web Master"))
                        positions.Add("Administrator");

                    roles.AddRange(positions);
                    roles.Add(member.MemberStatus.StatusName);
                }
                catch
                {
                }
            }

            return roles.ToArray();
        }

        public override string[] GetUsersInRole(string positionName)
        {
            var users = new List<string>();

            using (var db = new SphinxDbContext())
            {
                try
                {
                    var membersInRole = from uir in db.Leaders
                                        where uir.Position.Name == positionName
                                        select uir;

                    if (membersInRole.Any())
                    {
                        users.AddRange(membersInRole.Select(mir => mir.Member.UserName));
                    }
                }
                catch
                {
                }
            }

            return users.ToArray();
        }

        public override bool IsUserInRole(string userName, string roleName)
        {
            var isValid = false;

            using (var db = new SphinxDbContext())
            {
                try
                {
                    // Member status check
                    if (db.MemberStatuses.Select(s => s.StatusName).Contains(roleName))
                    {
                        isValid = db.Users.Single(m => m.UserName == userName).MemberStatus.StatusName == roleName;
                    }
                    // Administrator check
                    else if (roleName == "Administrator")
                    {
                        var adminAppointments = db.Leaders
                            .Where(l => l.Position.Name == roleName && l.Member.UserName == userName);
                        isValid = adminAppointments.Any();
                        if (!isValid)
                        {
                            var now = DateTime.UtcNow;
                            var semesters = db.Semesters.ToList();
                            // "term" represents the semester with appointments that are currently in power.
                            // Note: the previous term is obtained with the opposite where clause.
                            var term = semesters
                                .Where(s => s.TransitionDate > now)
                                .OrderBy(s => s.DateStart)
                                .First();
                            isValid = term.Leaders
                                .Where(l =>
                                    l.Member.UserName == userName &&
                                    l.Position.Name == "Web Master")
                                .Any();
                        }
                    }
                    // Position check
                    else
                    {
                        var now = DateTime.UtcNow;
                        var semesters = db.Semesters.ToList();
                        // "term" represents the semester with appointments that are currently in power.
                        // Note: the previous term is obtained with the opposite where clause.
                        var term = semesters
                            .Where(s => s.TransitionDate > now)
                            .OrderBy(s => s.DateStart)
                            .First();

                        isValid = term.Leaders
                            .Where(l =>
                                l.Member.UserName == userName &&
                                l.Position.Name == roleName)
                            .Any();
                    }
                }
                catch
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        public override void RemoveUsersFromRoles(string[] userNames, string[] positionNames)
        {
            try
            {
                using (var db = new SphinxDbContext())
                {
                    foreach (var userName in userNames)
                    {
                        // find each member in users table
                        var member = (from u in db.Users
                                      where u.UserName == userName
                                      select u)
                                    .FirstOrDefault();

                        if (member == null) continue;

                        // find all roles that are contained in the roleNames
                        var allPositions = (from r in db.Roles select r).ToList();

                        var removePositionIds = new List<int>();

                        foreach (var position in allPositions)
                        {
                            removePositionIds.AddRange(from positionName in positionNames
                                                       where position.Name == positionName
                                                       select position.Id);
                        }

                        if (removePositionIds.Count <= 0) continue;
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
            }
        }

        public override bool RoleExists(string positionName)
        {
            var isValid = false;

            using (var db = new SphinxDbContext())
            {
                // check if role exits
                if (db.Roles.Any(r => r.Name == positionName))
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        private string GetConfigValue(string configValue, string defaultValue)
        {
            return String.IsNullOrEmpty(configValue) ? defaultValue : configValue;
        }
        private DateTime ConvertUtcToCst(DateTime utc)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, cstZone);
        }
    }
}