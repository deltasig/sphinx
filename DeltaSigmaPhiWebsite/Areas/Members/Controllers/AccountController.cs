namespace DeltaSigmaPhiWebsite.Areas.Members.Controllers
{
    using App_Start;
    using DeltaSigmaPhiWebsite.Controllers;
    using Edu.Controllers;
    using Entities;
    using Microsoft.AspNet.Identity;
    using Microsoft.Web.WebPages.OAuth;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using System.Web.Security;
    using WebMatrix.WebData;

    [Authorize]
    public class AccountController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(string userName, ManageMessageId? accountMessage, MajorsController.MajorsMessageId? majorMessage)
        {
            switch (accountMessage)
            {
                case ManageMessageId.AddLoginSuccess:
                case ManageMessageId.ChangePasswordSuccess:
                case ManageMessageId.RemoveLoginSuccess:
                case ManageMessageId.SetPasswordSuccess:
                    ViewBag.SuccessMessage = GetManageMessage(accountMessage);
                    break;
                case ManageMessageId.ChangePasswordFailure:
                case ManageMessageId.SetPasswordFailure:
                    ViewBag.FailMessage = GetManageMessage(accountMessage);
                    break;
            }
            switch (majorMessage)
            {
                case MajorsController.MajorsMessageId.UnassignSuccess:
                    ViewBag.SuccessMessage = MajorsController.GetResultMessage(majorMessage);
                    break;
            }

            if (!string.IsNullOrEmpty(userName))
            {
                if (!OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(userName)))
                    userName = WebSecurity.CurrentUserName;
            }
            else if (string.IsNullOrEmpty(userName))
            {
                userName = WebSecurity.CurrentUserName;
            }

            var member = await _db.Members.SingleAsync(m => m.UserName == userName);

            ViewBag.StatusMessage = GetManageMessage(accountMessage);
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(userName));
            var thisSemester = await GetThisSemesterAsync();

            var model = new AccountInformationModel
            {
                MemberInfo = member,
                ChangePasswordModel = new LocalPasswordModel(),
                ThisSemesterCourses = member.ClassesTaken.Where(c => c.SemesterId == thisSemester.SemesterId).ToList(),
                CurrentSemester = thisSemester
            };

            return View(model);
        }
        
        [HttpGet]
        public async Task<ActionResult> Edit(string userName, AccountChangeMessageId? message)
        {
            var member = string.IsNullOrEmpty(userName)
                ? await _db.Members.SingleAsync(m => m.UserName == WebSecurity.CurrentUserName)
                : await _db.Members.SingleAsync(m => m.UserName == userName);
            var model = new EditMemberInfoModel
            {
                Member = member,
                Semesters = await GetAllSemesterListAsync(),
                PledgeClasses = await GetPledgeClassListAsync(),
                Statuses = await GetStatusListAsync(),
                Members = await GetUserIdListAsFullNameWithNoneAsync(),
                ShirtSizes = GetShirtSizesSelectList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditMemberInfoModel model)
        {
            if(!User.IsInRole("Administrator") && !User.IsInRole("Secretary") && !User.IsInRole("Academics") && 
               !User.IsInRole("House Manager") && User.Identity.Name != model.Member.UserName)
            {
                RedirectToAction("Index");
            }

            model.Semesters = await GetSemesterListAsync();
            model.PledgeClasses = await GetPledgeClassListAsync();
            model.Statuses = await GetStatusListAsync();
            model.Members = await GetUserIdListAsFullNameWithNoneAsync();
            model.ShirtSizes = GetShirtSizesSelectList();

            if (!ModelState.IsValid)
            {
                ViewBag.FailMessage = GetAccountChangeMessage(AccountChangeMessageId.UpdateFailed);
                return View(model);
            }

            var member = await _db.Members.SingleAsync(m => m.UserId == model.Member.UserId);
            member.FirstName = model.Member.FirstName;
            member.LastName = model.Member.LastName;
            member.Email = model.Member.Email;
            member.Pin = model.Member.Pin;
            member.Room = model.Member.Room;
            member.StatusId = model.Member.StatusId;
            member.PledgeClassId = model.Member.PledgeClassId;
            member.ExpectedGraduationId = model.Member.ExpectedGraduationId;
            member.BigBroId = model.Member.BigBroId == 0 ? null : model.Member.BigBroId;
            member.ShirtSize = model.Member.ShirtSize;

            _db.Members.Attach(member);
            _db.Entry(member).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            ViewBag.SuccessMessage = GetAccountChangeMessage(AccountChangeMessageId.UpdateSuccess);

            return View(model);
        }

        #region Local Authentication

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName.ToLower(), model.Password, model.RememberMe))
            {
                if (string.IsNullOrEmpty(model.ReturnUrl))
                    return RedirectToAction("Index", "Home", new { area = "Sphinx" });
                return Redirect(model.ReturnUrl);
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
        [Authorize(Roles = "Administrator, Secretary, Director of Recruitment, New Member Education")]
        public async Task<ActionResult> Registration(RegistrationMessageId? message)
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
                    StatusList = await GetStatusListAsync(), 
                    PledgeClassList = await GetPledgeClassListAsync(),
                    SemesterList = await GetAllSemesterListAsync(),
                    ShirtSizes = GetShirtSizesSelectList()
                },
                UnregisterModel = new UnregisterModel
                {
                    Users = await GetUserIdListAsFullNameAsync()
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Secretary, Director of Recruitment, New Member Education")]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            RegistrationMessageId? message = RegistrationMessageId.RegistrationFailed;

            if (!ModelState.IsValid) return RedirectToAction("Registration", new { Message = message });
            // Attempt to register the user
            try
            {
                model.Email = model.Email.ToLower();
                model.UserName = model.UserName.ToLower();
                var tempPassword = Membership.GeneratePassword(10, 5);
                WebSecurity.CreateUserAndAccount(model.UserName, tempPassword, new { 
                        model.FirstName, model.LastName, model.Email, 
                        model.Room, model.StatusId, model.PledgeClassId, 
                        model.ExpectedGraduationId, model.ShirtSize
                });

                _db.Addresses.Add(new Address { UserId = WebSecurity.GetUserId(model.UserName), Type = "Mailing" });
                _db.Addresses.Add(new Address { UserId = WebSecurity.GetUserId(model.UserName), Type = "Permanent" });
                _db.PhoneNumbers.Add(new PhoneNumber { UserId = WebSecurity.GetUserId(model.UserName), Type = "Mobile" });
                _db.PhoneNumbers.Add(new PhoneNumber { UserId = WebSecurity.GetUserId(model.UserName), Type = "Emergency" });
                await _db.SaveChangesAsync();

                message = RegistrationMessageId.RegistrationSuccess;

                var emailMessage = new IdentityMessage
                {
                    Subject = "Registration Successful at deltasig-de.org",
                    Body = "You have been successfully registered on deltasig-de.org. Your username is " + model.UserName +
                        " and your temporary password (change it when you log in) is: " + tempPassword,
                    Destination = model.Email
                };
                
                try
                {
                    var emailService = new EmailService();
                    await emailService.SendAsync(emailMessage);
                }
                catch (SmtpException e)
                {

                }
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
        public async Task<ActionResult> Unregister(UnregisterModel model)
        {
            RegistrationMessageId? message;

            if (ModelState.IsValid)
            {
                // Attempt to remove the user from the system
                try
                {
                    var member = await _db.Members.SingleAsync(m => m.UserId == model.SelectedUserId);

                    if(member.ClassesTaken.Count > 0 || member.StudyHourAssignments.Count > 0 ||
                       member.Committees.Count > 0 || member.IncidentReports.Count > 0 ||
                       member.OrganizationsJoined.Count > 0 || member.LaundrySignups.Count > 0 ||
                       member.LittleBrothers.Count > 0 || member.MajorsToMember.Count > 0 ||
                       member.Leaders.Count > 0 || member.StudyHourApprovals.Count > 0)
                    {
                        message = RegistrationMessageId.UnregisterFailed;
                        return RedirectToAction("Registration", new { Message = message });
                    }

                    foreach (var address in member.Addresses.ToList())
                    {
                        _db.Addresses.Remove(address);
                    }
                    foreach (var number in member.PhoneNumbers.ToList())
                    {
                        _db.PhoneNumbers.Remove(number);
                    }
                    await _db.SaveChangesAsync();

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

                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordFailure });
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing OldPassword field
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
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordFailure });
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> ResetPassword(string userName)
        {
            bool changePasswordSucceeded;
            var tempPassword = Membership.GeneratePassword(10, 5);;
            try
            {
                var token = WebSecurity.GeneratePasswordResetToken(userName, 5);
                changePasswordSucceeded = WebSecurity.ResetPassword(token, tempPassword);
            }
            catch (Exception)
            {
                changePasswordSucceeded = false;
            }

            if (changePasswordSucceeded)
            {
                var user = await _db.Members.SingleAsync(m => m.UserName == userName);
                var emailMessage = new IdentityMessage
                {
                    Subject = "Account Password has been reset at deltasig-de.org",
                    Body = "Your account password has been reset on deltasig-de.org. Your username is " + userName +
                        " and your new temporary password (change it when you log in) is: " + tempPassword,
                    Destination = user.Email
                };

                try
                {
                    var emailService = new EmailService();
                    await emailService.SendAsync(emailMessage);
                }
                catch (SmtpException e)
                {

                }
                return RedirectToAction("Index", new { userName, Message = ManageMessageId.ChangePasswordSuccess });
            }

            return RedirectToAction("Index", new { userName, Message = ManageMessageId.ChangePasswordFailure });
        }

        #endregion
        
        #region Error Messages

        private static dynamic GetRegistrationMessage(RegistrationMessageId? message)
        {
            return message == RegistrationMessageId.RegistrationSuccess ? "The new account has been successfully registered."
                : message == RegistrationMessageId.RegistrationFailed ? "Account registration failed.  Please contact the system administrator."
                : message == RegistrationMessageId.UnregisterSuccess ? "The account has been successfully removed."
                : message == RegistrationMessageId.UnregisterFailed ? "Account removal failed.  This could be because the user has records in the system tied to them and can't be deleted."
                : "";
        }
        private static dynamic GetManageMessage(ManageMessageId? message)
        {
            return message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
            : message == ManageMessageId.AddLoginSuccess ? "External login was added."
            : message == ManageMessageId.SetPasswordFailure ? "The current password is incorrect or the new password is invalid."
            : message == ManageMessageId.ChangePasswordFailure ? "Failed to change password. Your old password was probably incorrect."
            : "";
        }
        private static dynamic GetAccountChangeMessage(AccountChangeMessageId? message)
        {
            return message == AccountChangeMessageId.UpdateSuccess ? "This account has been updated."
            : message == AccountChangeMessageId.UpdateFailed ? "Update failed, please verify that the data you entered is valid."
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
            ChangePasswordFailure,
            SetPasswordSuccess,
            SetPasswordFailure,
            RemoveLoginSuccess,
            AddLoginSuccess,
        }
        public enum RegistrationMessageId
        {
            RegistrationSuccess,
            UnregisterSuccess,
            RegistrationFailed,
            UnregisterFailed
        }

        public enum AccountChangeMessageId
        {
            UpdateSuccess,
            UpdateFailed
        }

        #endregion
    }
}
