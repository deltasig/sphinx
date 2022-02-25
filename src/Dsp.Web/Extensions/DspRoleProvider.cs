namespace Dsp.Web.Extensions
{
    using Dsp.Data;
    using Dsp.Services;
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading.Tasks;
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

        private string GetConfigValue(string configValue, string defaultValue)
        {
            return string.IsNullOrEmpty(configValue) ? defaultValue : configValue;
        }

        public override void AddUsersToRoles(string[] userNames, string[] positionNames)
        {
            throw new NotSupportedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotSupportedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotSupportedException();
        }

        public override string[] FindUsersInRole(string positionName, string usernameToMatch)
        {
            throw new NotSupportedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotSupportedException();
        }

        public override string[] GetRolesForUser(string userName)
        {
            using (var db = new SphinxDbContext())
            {
                var memberService = new MemberService(db);
                var positionService = new PositionService(db);
                var user = Task.Run(() => memberService.GetMemberByUserNameAsync(userName)).GetAwaiter().GetResult();
                var currentPositionsForUser = Task.Run(() => positionService.GetCurrentPositionsByUserAsync(userName)).GetAwaiter().GetResult();
                var roles = currentPositionsForUser
                    .Select(x => x.Name)
                    .ToList();
                roles.Add(user.MemberStatus.StatusName);

                return roles.ToArray();
            }
        }

        public override string[] GetUsersInRole(string positionName)
        {
            throw new NotSupportedException();
        }

        public override bool IsUserInRole(string userName, string roleName)
        {
            throw new NotSupportedException();
        }

        public override void RemoveUsersFromRoles(string[] userNames, string[] positionNames)
        {
            throw new NotSupportedException();
        }

        public override bool RoleExists(string positionName)
        {
            throw new NotSupportedException();
        }
    }
}