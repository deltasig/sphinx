﻿namespace DeltaSigmaPhiWebsite.Data
{
    using System;
    using System.Linq;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlClient;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Security;
    using System.Collections.Generic;
    using Models;
    using DeltaSigmaPhiWebsite.Models.Entities;

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
                using (var db = new DspContext())
                {
                    foreach (var userName in userNames)
                    {
                        // find each user in users table
                        var name = userName;
                        var user = (from m in db.Members
                                    where m.UserName == name
                                    select m).FirstOrDefault();

                        if (user == null) continue;
                        // find all roles that are contained in the roleNames
                        var allPositions = (from p in db.Positions select p).ToList();

                        var positionIds = new List<int>();

                        foreach (var position in allPositions)
                        {
                            positionIds.AddRange(from positionName in positionNames 
                                                 where position.PositionName == positionName 
                                                 select position.PositionId);
                        }

                        if (positionIds.Count <= 0) continue;

                        foreach (var positionId in positionIds)
                        {
                            var memberInRole = (from mip in db.Leaders
                                                where mip.Member.UserId == user.UserId && 
                                                      mip.PositionId == positionId
                                                select mip).FirstOrDefault();
                            if (memberInRole == null)
                            {
                                memberInRole = new Leader
                                {
                                    UserId = user.UserId,
                                    PositionId = positionId,
                                    SemesterId = (from s in db.Semesters
                                                  orderby s.DateEnd descending
                                                  select s.SemesterId).First()
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

        public override void CreateRole(string roleName)
        {
            try
            {
                using (var db = new DspContext())
                {
                    db.Positions.Add(new Position
                    {
                        PositionName = roleName
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

            using (var db = new DspContext())
            {
                try
                {
                    var position = (from pos in db.Positions
                                    where pos.PositionName == roleName
                                    select pos).SingleOrDefault();

                    if (position != null)
                    {
                        db.Positions.Remove(position);
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

            using (var db = new DspContext())
            {
                try
                {
                    var usersInRole = from mip in db.Leaders
                                      where mip.Position.PositionName == positionName && 
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

            using (var db = new DspContext())
            {
                try
                {
                    var positions = from r in db.Positions
                                  select r;

                    roles.AddRange(positions.Select(p => p.PositionName));
                }
                catch
                {
                }
            }

            return roles.ToArray();
        }

        public override string[] GetRolesForUser(string username)
        {
            var roles = new List<string>();

            using (var db = new DspContext())
            {
                try
                {
                    var positionsHeld = from r in db.Leaders
                                  where r.Member.UserName == username
                                  select r;

                    roles.AddRange(positionsHeld.Select(p => p.Position.PositionName));
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

            using (var db = new DspContext())
            {
                try
                {
                    var membersInRole = from uir in db.Leaders
                                        where uir.Position.PositionName == positionName
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

        public override bool IsUserInRole(string userName, string positionName)
        {
            var isValid = false;

            using (var db = new DspContext())
            {
                try
                {
                    var membersInRole = from mip in db.Leaders
                                        where mip.Position.PositionName == positionName &&
                                              mip.Member.UserName == userName
                                        select mip;
                    if (membersInRole.Any())
                    {
                        isValid = true;
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
                using (var db = new DspContext())
                {
                    foreach (var userName in userNames)
                    {
                        // find each member in users table
                        var member = (from u in db.Members
                                     where u.UserName == userName
                                     select u)
                                    .FirstOrDefault();

                        if (member == null) continue;

                        // find all roles that are contained in the roleNames
                        var allPositions = (from r in db.Positions select r).ToList();

                        var removePositionIds = new List<int>();

                        foreach (var position in allPositions)
                        {
                            removePositionIds.AddRange(from positionName in positionNames 
                                                       where position.PositionName == positionName 
                                                       select position.PositionId);
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

            using (var db = new DspContext())
            {
                // check if role exits
                if (db.Positions.Any(r => r.PositionName == positionName))
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
    }
}