namespace DeltaSigmaPhiWebsite.Controllers
{
    using Data;
    using Data.UnitOfWork;
    using Models;
    using Models.Entities;
    using Models.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;
    using System.Web.Mvc;
    using System.Web.Security;
    using WebMatrix.WebData;

    [Authorize(Roles = "Pledge, Neophyte, Active, Alumnus, Affiliate")]
    public class AccountController : BaseController
    {
        public AccountController(IUnitOfWork uow, IWebSecurity ws, IOAuthWebSecurity oaws) : base(uow, ws, oaws) { }

        [HttpGet]
        public ActionResult Index(string userName, ManageMessageId? message)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                if (!OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(userName)))
                    userName = WebSecurity.CurrentUser.Identity.Name;
            }
            else if (string.IsNullOrEmpty(userName))
            {
                userName = WebSecurity.CurrentUser.Identity.Name;
            }

            var member = uow.MemberRepository.Single(m => m.UserName == userName);

            ViewBag.StatusMessage = GetManageMessage(message);
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(userName));

            var model = new AccountInformationModel
            {
                MemberInfo = member,
                ChangePasswordModel = new LocalPasswordModel()
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult Roster(RosterModel model)
        {
            if (model.SearchModel != null)
            {
                if (!string.IsNullOrEmpty(model.SearchModel.SearchTerm))
                {
                    model.Members = uow.MemberRepository
                        .SelectAll()
                        .Where(m => m.FirstName.ToLower().Contains(model.SearchModel.SearchTerm.ToLower()) ||
                            m.LastName.ToLower().Contains(model.SearchModel.SearchTerm.ToLower()))
                        .ToList();
                }
                else if (model.SearchModel.CustomSearchRequested())
                {
                    IEnumerable<Member> guidedSearchResults = uow.MemberRepository
                        .SelectAll()
                        .OrderBy(o => o.LastName)
                        .ToList();
                    if (model.SearchModel.SelectedStatusId != -1)
                    {
                        guidedSearchResults =
                            guidedSearchResults.Where(m => m.StatusId == model.SearchModel.SelectedStatusId);
                    }
                    if (model.SearchModel.SelectedPledgeClassId != -1)
                    {
                        guidedSearchResults =
                            guidedSearchResults.Where(m => m.PledgeClassId == model.SearchModel.SelectedPledgeClassId);
                    }
                    if (model.SearchModel.SelectedGraduationSemesterId != -1)
                    {
                        guidedSearchResults =
                            guidedSearchResults.Where(m => m.ExpectedGraduationId == model.SearchModel.SelectedGraduationSemesterId);
                    }
                    switch (model.SearchModel.LivingType)
                    {
                        case "InHouse":
                            guidedSearchResults =
                                guidedSearchResults.Where(m => m.Room != 0);
                            break;
                        case "OutOfHouse":
                            guidedSearchResults =
                                guidedSearchResults.Where(m => m.Room == 0);
                            break;
                    }

                    model.Members = guidedSearchResults.ToList();
                }
                else
                {
                    model.Members = uow.MemberRepository
                        .SelectAll()
                        .Where(m => m.MemberStatus.StatusName == "Active")
                        .OrderBy(o => o.PledgeClassId)
                        .ThenBy(o => o.LastName)
                        .ToList();
                }
            }
            else
            {
                model = new RosterModel
                {
                    Members = uow.MemberRepository
                        .SelectAll()
                        .Where(m => m.MemberStatus.StatusName == "Active")
                        .OrderBy(o => o.PledgeClassId)
                        .ThenBy(o => o.LastName)
                        .ToList()
                };
            }

            model.SearchModel = new RosterSearchModel
            {
                Statuses = GetStatusListWithNone(),
                PledgeClasses = GetPledgeClassListWithNone(),
                Semesters = GetSemesterListWithNone(),
                LivingType = "Both"
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Secretary, Academics")]
        public ActionResult Edit(string userName)
        {
            var member = string.IsNullOrEmpty(userName)
                ? uow.MemberRepository.Single(m => m.UserName == WebSecurity.CurrentUser.Identity.Name)
                : uow.MemberRepository.Single(m => m.UserName == userName);
            var model = new EditMemberInfoModel
            {
                Member = member,
                Semesters = GetSemesterList(),
                PledgeClasses = GetPledgeClassList(),
                Statuses = GetStatusList(),
                Members = GetUserIdListAsFullNameWithNone()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Secretary, Academics")]
        public ActionResult Edit(EditMemberInfoModel model)
        {
            model.Semesters = GetSemesterList();
            model.PledgeClasses = GetPledgeClassList();
            model.Statuses = GetStatusList();
            model.Members = GetUserIdListAsFullNameWithNone();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var member = uow.MemberRepository.Single(m => m.UserId == model.Member.UserId);
            member.Pin = model.Member.Pin;
            member.Room = model.Member.Room;
            member.StatusId = model.Member.StatusId;
            member.PledgeClassId = model.Member.PledgeClassId;
            member.ExpectedGraduationId = model.Member.ExpectedGraduationId;
            member.BigBroId = model.Member.BigBroId == 0 ? null : model.Member.BigBroId;
            member.RequiredStudyHours = model.Member.RequiredStudyHours;

            uow.MemberRepository.Update(member);
            uow.Save();

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
                if (string.IsNullOrEmpty(returnUrl))
                    return RedirectToAction("Index", "Sphinx");
                return RedirectToAction(returnUrl);
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

            return RedirectToAction("Login", "Account");
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
                RegisterModel = new RegisterModel
                {
                    StatusList = GetStatusList(), 
                    PledgeClassList = GetPledgeClassList(),
                    SemesterList = GetSemesterList()
                },
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

            if (!ModelState.IsValid) return RedirectToAction("Registration", new { Message = message });
            // Attempt to register the user
            try
            {
                model.Email = model.Email.ToLower();
                var signIndex = model.Email.IndexOf('@');
                var userName = model.Email.Substring(0, signIndex);

                WebSecurity.CreateUserAndAccount(userName.ToLower(), model.Password,
                    new { model.FirstName, model.LastName, model.Email, model.Room, model.StatusId, model.PledgeClassId, model.ExpectedGraduationId, RequiredStudyHours = 0 });

                uow.AddressRepository.Insert(new Address { UserId = WebSecurity.GetUserId(userName), Type = "Mailing" });
                uow.AddressRepository.Insert(new Address { UserId = WebSecurity.GetUserId(userName), Type = "Permanent" });
                uow.PhoneNumberRepository.Insert(new PhoneNumber { UserId = WebSecurity.GetUserId(userName), Type = "Mobile" });
                uow.PhoneNumberRepository.Insert(new PhoneNumber { UserId = WebSecurity.GetUserId(userName), Type = "Emergency" });
                uow.Save();

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
                    var member = uow.MemberRepository.Single(m => m.UserId == model.SelectedUserId);
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
                if (!ModelState.IsValid) return RedirectToAction("Index");
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

                if (!ModelState.IsValid) return RedirectToAction("Index");
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
        [Authorize(Roles = "Administrator, President")]
        public ActionResult Appointments(AppointmentMessageId? message)
        {
            switch (message)
            {
                case AppointmentMessageId.AppointmentSuccess:
                    ViewBag.SuccessMessage = GetAppointmentMessage(message);
                    break;
                case AppointmentMessageId.AppointmentFailure:
                    ViewBag.FailMessage = GetAppointmentMessage(message);
                    break;
            }

            var positions = uow.PositionRepository.SelectAll().ToList();
            var semesters = GetThisAndNextSemesterList().ToList();
            var model = new List<AppointmentModel>();

            foreach(var position in positions)
            {
                if (position.PositionName == "Administrator") continue;
                foreach(var semester in semesters)
                {
                    var leader = uow.LeaderRepository.SelectAll().ToList().SingleOrDefault(l =>
                        l.SemesterId == semester.SemesterId &&
                        l.PositionId == position.PositionId) ?? new Leader();
                    model.Add(new AppointmentModel
                    {
                        Leader = new Leader
                        {
                            Position = position,
                            PositionId = position.PositionId,
                            Semester = semester,
                            SemesterId = semester.SemesterId,
                            UserId = leader.UserId 
                        },
                        Users = GetUserIdListAsFullNameWithNoneNonSelectList()
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, President")]
        public ActionResult Appointments(IList<AppointmentModel> model)
        {
            AppointmentMessageId? message = null;

            try
            {
                foreach (var ap in model)
                {
                    // Check if a Leader entry already exists.
                    var leader = uow.LeaderRepository.Single(m =>
                        m.SemesterId == ap.Leader.SemesterId &&
                        m.PositionId == ap.Leader.PositionId);

                    if (leader == null)
                    {
                        if (ap.Leader.UserId == 0) continue;
                        ((DspRoleProvider)Roles.Provider)
                            .AddUserToRole(ap.Leader.UserId, ap.Leader.PositionId, ap.Leader.SemesterId);
                    }
                    else if (ap.Leader.UserId == 0)
                    {
                        uow.LeaderRepository.DeleteById(leader.LeaderId);
                        uow.Save();
                    }
                    else if (ap.Leader.UserId != 0)
                    {
                        leader.UserId = ap.Leader.UserId;
                        uow.LeaderRepository.Update(leader);
                        uow.Save();
                    }
                }
                message = AppointmentMessageId.AppointmentSuccess;
            }
            catch (Exception)
            {
                message = AppointmentMessageId.AppointmentFailure;
            }

            return RedirectToAction("Appointments", new { Message = message });
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
        private static dynamic GetAppointmentMessage(AppointmentMessageId? message)
        {
            return 
                message == AppointmentMessageId.AppointmentSuccess ? "Appointments completed."
                : message == AppointmentMessageId.AppointmentFailure ? "Appointments failed. Please check your appointments and try again."
                : "";
        }
        private static dynamic GetManageMessage(ManageMessageId? message)
        {
            return message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
            : message == ManageMessageId.AddLoginSuccess ? "External login was added."
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
            AddLoginSuccess,
        }
        public enum AppointmentMessageId
        {
            AppointmentSuccess,
            AppointmentFailure
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
