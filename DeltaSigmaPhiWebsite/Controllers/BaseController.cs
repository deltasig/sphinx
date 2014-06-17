namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.UnitOfWork;
    using Models;
    using Models.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using System.Web.Security;

    public class BaseController : Controller
    {
        internal IWebSecurity WebSecurity { get; set; }
        internal IOAuthWebSecurity OAuthWebSecurity { get; set; }

        protected readonly IUnitOfWork uow = new UnitOfWork();
        public BaseController(IUnitOfWork uow, IWebSecurity webSecurity, IOAuthWebSecurity oAuthWebSecurity)
        {
            this.uow = uow;
            this.WebSecurity = webSecurity;
            this.OAuthWebSecurity = oAuthWebSecurity;
        }

        protected virtual int? GetThisSemestersId()
        {
            var semesters = uow.SemesterRepository.GetAll().ToList();
            if (semesters.Count <= 0) return null;

            var thisSemester = uow.SemesterRepository.GetAll().First(s => 
                s.DateStart <= DateTime.Now &&
                s.DateEnd >= DateTime.Now);
            if (thisSemester != null)
                return thisSemester.SemesterId;

            return null;
        }
        protected virtual IEnumerable<SelectListItem> GetUserIdListAsFullName()
        {
            var members = uow.MemberRepository.GetAll().OrderBy(o => o.LastName);
            var newList = new List<object>();
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.LastName + ", " + member.FirstName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual IEnumerable<SelectListItem> GetUserIdListAsFullNameWithNone()
        {
            var members = uow.MemberRepository.GetAll().OrderBy(o => o.LastName);
            var newList = new List<object> { new { UserId = 0, Name = "None" } };
            foreach (var member in members)
            {
                newList.Add(new
                {
                    member.UserId,
                    Name = member.LastName + ", " + member.FirstName
                });
            }
            return new SelectList(newList, "UserId", "Name");
        }
        protected virtual IEnumerable<SelectListItem> GetStatusList()
        {
            var statusList = uow.MemberStatusRepository.GetAll();
            var newList = new List<object>();
            foreach (var status in statusList)
            {
                newList.Add(new
                {
                    status.StatusId,
                    status.StatusName
                });
            }
            return new SelectList(newList, "StatusId", "StatusName");
        }
        protected virtual IEnumerable<SelectListItem> GetTerms()
        {
            var terms = new List<string>
            {
                "Spring", "Fall"
            };
            return new SelectList(terms);
        }
        protected virtual IEnumerable<SelectListItem> GetRoleList()
        {
            var roles = Roles.GetAllRoles();
            return new SelectList(roles);
        }
        protected virtual IEnumerable<ExternalLogin> GetExternalLogins(string userName)
        {
            var accounts = OAuthWebSecurity.GetAccountsFromUserName(userName);
            var externalLogins = (
                from account in accounts
                let clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider)
                select new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId
                }
            ).ToList();
            return externalLogins;
        }
        public virtual string GetPictureUrl(string userName)
        {
            var logins = GetExternalLogins(userName).ToList();
            if (logins.Count <= 0) return "";
            var facebookId = (from login in logins where login.ProviderDisplayName == "Facebook" select login.ProviderUserId).First();

            WebResponse response = null;
            var pictureUrl = string.Empty;
            try
            {
                var request = WebRequest.Create(string.Format("https://graph.facebook.com/{0}/picture", facebookId));
                response = request.GetResponse();
                pictureUrl = response.ResponseUri.ToString();
            }
            catch (Exception ex)
            {
                //? handle
            }
            finally
            {
                if (response != null) response.Close();
            }
            return pictureUrl;
        }
        public virtual string GetBigPictureUrl(string userName)
        {
            var logins = GetExternalLogins(userName).ToList();
            if (logins.Count <= 0) return "";
            var facebookId = (from login in logins where login.ProviderDisplayName == "Facebook" select login.ProviderUserId).First();

            WebResponse response = null;
            var pictureUrl = string.Empty;
            try
            {
                var request = WebRequest.Create(string.Format("https://graph.facebook.com/{0}/picture?type=large", facebookId));
                response = request.GetResponse();
                pictureUrl = response.ResponseUri.ToString();
            }
            catch (Exception ex)
            {
                //? handle
            }
            finally
            {
                if (response != null) response.Close();
            }
            return pictureUrl;
        }

        protected override void Dispose(bool disposing)
        {
            uow.Dispose();
            base.Dispose(disposing);
        }
    }
}