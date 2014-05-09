namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data.Interfaces;
    using Data.UnitOfWork;
    using Microsoft.Web.WebPages.OAuth;
    using Models;
    using System;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using System.Web.Security;
    using WebMatrix.WebData;

    [Authorize]
    public class AccountController : BaseController
    {
        public AccountController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        [HttpGet]
        public ActionResult Index(ManageMessageId? message)
        {
            var userName = WebSecurity.CurrentUser.Identity.Name;
            var member = uow.MemberRepository.Get(m => m.UserName == userName);
            var model = new AccountInformationModel
            {
                MemberInfo = member,
                ProfilePicUrl = GetBigPictureUrl(member.UserName),
                ChangePasswordModel = new LocalPasswordModel()
            };

            ViewBag.StatusMessage = GetManageMessage(message);
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(userName));
            return View(model);
        }

        #region Local Authentication

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, model.RememberMe))
            {
                return RedirectToAction("Index", "Sphinx");
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Secretary")]
        public ActionResult Registration(RegistrationMessageId? message)
        {
            switch (message)
            {
                case RegistrationMessageId.RegistrationSuccess:
                case RegistrationMessageId.UnregisterSuccess:
                    ViewBag.SuccessMessage = GetRegistrationMessage(message);
                    break;
                case RegistrationMessageId.RegistrationFailed:
                case RegistrationMessageId.UnregisterFailed:
                    ViewBag.FailMessage = GetRegistrationMessage(message);
                    break;
            }
            var model = new RegistrationModel
            {
                RegisterModel = new RegisterModel { StatusList = GetStatusList() },
                UnregisterModel = new UnregisterModel { Users = GetUserIdListAsFullName() }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Secretary")]
        public ActionResult Register(RegisterModel model)
        {
            RegistrationMessageId? message = RegistrationMessageId.RegistrationFailed;

            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    model.Email = model.Email.ToLower();
                    var signIndex = model.Email.IndexOf('@');
                    var userName = model.Email.Substring(0, signIndex);

                    WebSecurity.CreateUserAndAccount(userName.ToLower(), model.Password,
                        new { model.FirstName, model.LastName, NickName = model.Nickname, model.Email, model.StatusId });

                    message = RegistrationMessageId.RegistrationSuccess;
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Something went wrong.  Sorry for the inconvenience.");
                }
            }
            return RedirectToAction("Registration", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Unregister(UnregisterModel model)
        {
            RegistrationMessageId? message;

            if (ModelState.IsValid)
            {
                // Attempt to remove the user from the system
                try
                {
                    var member = uow.MemberRepository.Get(m => m.UserId == model.SelectedUserId);
                    ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(member.UserName);
                    // deletes record from webpages_Membership table
                    (Membership.Provider).DeleteUser(member.UserName, true);

                    message = RegistrationMessageId.UnregisterSuccess;
                    return RedirectToAction("Registration", new { Message = message });
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
                catch (InvalidOperationException)
                {
                    ModelState.AddModelError("", "Invalid Submission.");
                }
            }
            message = RegistrationMessageId.UnregisterFailed;
            return RedirectToAction("Registration", new { Message = message });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(LocalPasswordModel model)
        {
            var hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Index");
            if (hasLocalAccount)
            {
                if (!ModelState.IsValid) return View(model);
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
                }

                ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                var state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (!ModelState.IsValid) return View(model);
                try
                {
                    WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("Index");
        }

        #endregion

        #region Roles

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult RoleManager(RoleMessageId? message)
        {
            switch (message)
            {
                case RoleMessageId.CreateSuccess:
                case RoleMessageId.DeleteSuccess:
                case RoleMessageId.RoleAssignSuccess:
                case RoleMessageId.RoleUnassignSuccess:
                    ViewBag.SuccessMessage = GetRoleMessage(message);
                    break;
                case RoleMessageId.CreateDuplicate:
                case RoleMessageId.DeleteNonExistent:
                case RoleMessageId.RoleAssignDuplicate:
                case RoleMessageId.RoleUnassignFailed:
                    ViewBag.FailMessage = GetRoleMessage(message);
                    break;
            }

            var model = new RoleManagerModel
            {
                AssignModel = new AssignUserToRoleModel { Users = GetUserIdListAsFullName(), Roles = GetRoleList() },
                UnassignModel = new UnassignUserFromRoleModel { Users = GetUserIdListAsFullName(), Roles = GetRoleList() },
                CreateModel = new CreateRoleModel(),
                DeleteModel = new DeleteRoleModel { Roles = GetRoleList() }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult AssignUserToRole(AssignUserToRoleModel model)
        {
            RoleMessageId? message = null;
            var member = uow.MemberRepository.Get(m => m.UserId == model.SelectedUserId);
            if (Roles.IsUserInRole(member.UserName, model.SelectedRoleName))
            {
                message = RoleMessageId.RoleAssignDuplicate;
            }
            else
            {
                Roles.AddUserToRole(member.UserName, model.SelectedRoleName);
                message = RoleMessageId.RoleAssignSuccess;
            }

            return RedirectToAction("RoleManager", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult UnassignUserFromRole(UnassignUserFromRoleModel model)
        {
            RoleMessageId? message = null;
            var member = uow.MemberRepository.Get(m => m.UserId == model.SelectedUserId);
            if (Roles.IsUserInRole(member.UserName, model.SelectedRoleName))
            {
                Roles.RemoveUserFromRole(member.UserName, model.SelectedRoleName);
                message = RoleMessageId.RoleUnassignSuccess;
            }
            else
            {
                message = RoleMessageId.RoleUnassignFailed;
            }

            return RedirectToAction("RoleManager", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateRole(CreateRoleModel model)
        {
            RoleMessageId? message;

            if (!Roles.RoleExists(model.RoleName))
            {
                Roles.CreateRole(model.RoleName);
                message = RoleMessageId.CreateSuccess;
            }
            else
            {
                message = RoleMessageId.CreateDuplicate;
            }

            return RedirectToAction("RoleManager", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteRole(DeleteRoleModel model)
        {
            RoleMessageId? message;

            if (Roles.RoleExists(model.SelectedRoleName))
            {
                Roles.DeleteRole(model.SelectedRoleName);
                message = RoleMessageId.DeleteSuccess;
            }
            else
            {
                message = RoleMessageId.DeleteNonExistent;
            }

            return RedirectToAction("RoleManager", new { Message = message });
        }

        #endregion

        #region External Authentication

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            var ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount != User.Identity.Name)
                return RedirectToAction("Index", new { Message = message });

            // Use a transaction to prevent the user from deleting their last login credential
            using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            {
                var hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                if (!hasLocalAccount && OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count <= 1)
                    return RedirectToAction("Index", new { Message = message });
                OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                scope.Complete();
                message = ManageMessageId.RemoveLoginSuccess;
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider)
        {
            return new ExternalLoginResult(this, provider, Url.Action("ExternalLoginCallback"));
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ExternalLoginCallback()
        {
            var result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback"));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, false))
            {
                return RedirectToAction("Index", "Sphinx");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            // If the current user is logged in, add the new account
            OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
            return RedirectToAction("Index", "Sphinx");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            var accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);

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

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(BaseController controller, string provider, string returnUrl)
            {
                Controller = controller;
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public BaseController Controller { get; set; }
            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                Controller.OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        #endregion

        #region Error Messages

        private static dynamic GetRegistrationMessage(RegistrationMessageId? message)
        {
            return message == RegistrationMessageId.RegistrationSuccess ? "The new account has been successfully registered."
                : message == RegistrationMessageId.RegistrationFailed ? "Account registration failed.  Please contact the system administrator."
                : message == RegistrationMessageId.UnregisterSuccess ? "The account has been successfully removed."
                : message == RegistrationMessageId.UnregisterFailed ? "Account removal failed.  Please contact the system administrator."
                : "";
        }
        private static dynamic GetRoleMessage(RoleMessageId? message)
        {
            return message == RoleMessageId.CreateSuccess ? "Role created."
                : message == RoleMessageId.DeleteSuccess ? "Role deleted."
                : message == RoleMessageId.CreateDuplicate ? "Creation failed because the role already exists."
                : message == RoleMessageId.DeleteNonExistent ? "Delete failed because no role with that name exists."
                : message == RoleMessageId.RoleAssignSuccess ? "User successfully assigned to role."
                : message == RoleMessageId.RoleAssignDuplicate ? "User is already in that role."
                : message == RoleMessageId.RoleUnassignSuccess ? "User removed from role."
                : message == RoleMessageId.RoleUnassignFailed ? "User is not in role and cannot be removed from it."
                : "";
        }
        private static dynamic GetManageMessage(ManageMessageId? message)
        {
            return message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
            : "";
        }
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }
        public enum RoleMessageId
        {
            RoleAssignSuccess,
            RoleUnassignSuccess,
            CreateSuccess,
            DeleteSuccess,
            CreateDuplicate,
            DeleteNonExistent,
            RoleAssignDuplicate,
            RoleUnassignFailed,
        }
        public enum RegistrationMessageId
        {
            RegistrationSuccess,
            UnregisterSuccess,
            RegistrationFailed,
            UnregisterFailed
        }

        #endregion

    }
}
