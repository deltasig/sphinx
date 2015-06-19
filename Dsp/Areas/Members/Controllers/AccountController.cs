namespace Dsp.Areas.Members.Controllers
{
    using Edu.Controllers;
    using Entities;
    using Extensions;
    using global::Dsp.Controllers;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;

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
                case ManageMessageId.ChangePasswordMismatch:
                case ManageMessageId.ChangePasswordWrongOriginal:
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

            if (string.IsNullOrEmpty(userName))
            {
                userName = User.Identity.GetUserName();
            }

            var member = await UserManager.FindByNameAsync(userName);

            ViewBag.StatusMessage = GetManageMessage(accountMessage);
            var thisSemester = await GetThisSemesterAsync();

            var model = new AccountInformationModel
            {
                MemberInfo = member,
                ChangePasswordModel = new LocalPasswordModel(),
                ThisSemesterCourses = member.ClassesTaken
                    .Where(c => c.SemesterId == thisSemester.SemesterId).ToList(),
                CurrentSemester = thisSemester
            };

            return View(model);
        }
        
        [HttpGet]
        public async Task<ActionResult> Edit(string userName, AccountChangeMessageId? message)
        {
            var member = string.IsNullOrEmpty(userName)
                ? await UserManager.FindByNameAsync(User.Identity.Name)
                : await UserManager.FindByNameAsync(userName);
            var model = new EditMemberInfoModel
            {
                Member = member,
                Semesters = await GetAllSemesterListAsync(),
                PledgeClasses = await GetPledgeClassListAsync(),
                Statuses = await GetStatusListAsync(),
                Members = await GetAllUserIdsSelectListAsFullNameWithNoneAsync(),
                ShirtSizes = GetShirtSizesSelectList()
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditMemberInfoModel model)
        {
            if(!User.IsInRole("Administrator") && !User.IsInRole("Secretary") && !User.IsInRole("Academics") && 
               !User.IsInRole("House Manager") && User.Identity.Name != model.Member.UserName)
            {
                RedirectToAction("Index");
            }

            model.Semesters = await GetAllSemesterListAsync();
            model.PledgeClasses = await GetPledgeClassListAsync();
            model.Statuses = await GetStatusListAsync();
            model.Members = await GetAllUserIdsSelectListAsFullNameWithNoneAsync();
            model.ShirtSizes = GetShirtSizesSelectList();

            if (!ModelState.IsValid)
            {
                ViewBag.FailMessage = GetAccountChangeMessage(AccountChangeMessageId.UpdateFailed);
                return View(model);
            }

            var member = await UserManager.FindByIdAsync(model.Member.Id);
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

            _db.Users.Attach(member);
            _db.Entry(member).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            ViewBag.SuccessMessage = GetAccountChangeMessage(AccountChangeMessageId.UpdateSuccess);

            return View(model);
        }

        [HttpGet, AllowAnonymous]
        public ActionResult SignIn()
        {
            return View(new LoginModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> SignIn(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.UserName);
            if (!UserManager.IsEmailConfirmed(user.Id))
            {
                return RedirectToAction("Index", "Home");
            }
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);
            switch (result)
            {
                case SignInStatus.Success:
                    if (string.IsNullOrEmpty(model.ReturnUrl))
                    return RedirectToAction("Index", "Home", new { area = "Sphinx" });
                return Redirect(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { model.ReturnUrl, model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SignOut()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("SignIn", "Account");
        }

        [HttpGet, Authorize(Roles = "Administrator, Secretary, Director of Recruitment, New Member Education")]
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

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Secretary, Director of Recruitment, New Member Education")]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            RegistrationMessageId? message = RegistrationMessageId.RegistrationFailed;

            if (!ModelState.IsValid) return RedirectToAction("Registration", new { Message = message });
            // Attempt to register the user
            try
            {
                var tempPassword = Membership.GeneratePassword(10, 5);
                //WebSecurity.CreateUserAndAccount(model.UserName, tempPassword, new { 
                //        model.FirstName, model.LastName, model.Email, 
                //        model.Room, model.StatusId, model.PledgeClassId, 
                //        model.ExpectedGraduationId, model.ShirtSize
                //});
                var user = new Member
                {
                    UserName = model.UserName.ToLower(),
                    Email = model.Email.ToLower(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    StatusId = int.Parse(model.StatusId),
                    PledgeClassId = int.Parse(model.PledgeClassId),
                    ExpectedGraduationId = int.Parse(model.ExpectedGraduationId),
                    ShirtSize = model.ShirtSize
                };
                var result = await UserManager.CreateAsync(user, tempPassword);
                if (result.Succeeded)
                {
                    user = await UserManager.FindByNameAsync(user.UserName);

                    _db.Addresses.Add(new Address { UserId = user.Id, Type = "Mailing" });
                    _db.Addresses.Add(new Address { UserId = user.Id, Type = "Permanent" });
                    _db.PhoneNumbers.Add(new PhoneNumber { UserId = user.Id, Type = "Mobile" });
                    _db.PhoneNumbers.Add(new PhoneNumber { UserId = user.Id, Type = "Emergency" });
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
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
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

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Unregister(UnregisterModel model)
        {
            RegistrationMessageId? message;

            if (ModelState.IsValid)
            {
                // Attempt to remove the user from the system
                try
                {
                    var member = await UserManager.FindByIdAsync(model.SelectedUserId);

                    if(member.ClassesTaken.Count > 0 || member.IncidentReports.Count > 0 ||
                       member.LaundrySignups.Count > 0 || member.LittleBrothers.Count > 0 || 
                       member.MajorsToMember.Count > 0 || member.Leaders.Count > 0)
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

                    await UserManager.DeleteAsync(member);

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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(LocalPasswordModel model)
        {
            ViewBag.ReturnUrl = Url.Action("Index");

            if (!ModelState.IsValid) return RedirectToAction("Index");
            if (model.NewPassword != model.ConfirmPassword) 
                return RedirectToAction("Index", new { accountMessage = ManageMessageId.ChangePasswordMismatch });

            var userId = User.Identity.GetUserId<int>();
            var result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            return RedirectToAction("Index", result.Succeeded 
                ? new { accountMessage = ManageMessageId.ChangePasswordSuccess } 
                : new { accountMessage = ManageMessageId.ChangePasswordWrongOriginal });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> ResetPassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var member = await UserManager.FindByIdAsync((int)id);
            if (member == null)
            {
                return HttpNotFound();
            }
            return View(member);
        }
        
        [Authorize(Roles = "Administrator"), HttpPost, ValidateAntiForgeryToken, ActionName("ResetPassword")]
        public async Task<ActionResult> ResetPasswordConfirmed(Member member)
        {
            var tempPassword = Membership.GeneratePassword(10, 0);
            var token = await UserManager.GeneratePasswordResetTokenAsync(member.Id);
            var result = await UserManager.ResetPasswordAsync(member.Id, token, tempPassword);

            if (result.Succeeded)
            {
                var user = await UserManager.FindByNameAsync(member.UserName);
                var emailMessage = new IdentityMessage
                {
                    Subject = "Account Password has been reset at deltasig-de.org",
                    Body = "Your account password has been reset on deltasig-de.org. Your username is " + member.UserName +
                        " and your new temporary password (change it when you sign in) is: " + tempPassword,
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
                return RedirectToAction("Index", new { member.UserName, accountMessage = ManageMessageId.ResetPasswordSuccess });
            }
            AddErrors(result);

            return RedirectToAction("Index", new { member.UserName, accountMessage = ManageMessageId.ResetPasswordFailure });
        }
        
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
            return message == ManageMessageId.ChangePasswordSuccess ? "The password change for this account was successful."
            : message == ManageMessageId.ChangePasswordWrongOriginal ? "The password change failed because the wrong original password was entered."
            : message == ManageMessageId.ChangePasswordMismatch ? "The password change failed because the new password fields did not match."
            : message == ManageMessageId.ChangePasswordFailure ? "The password change failed for an unknown reason.  Please contact your administrator."

            : message == ManageMessageId.SetPasswordSuccess ? "The password for this account password has been set."
            : message == ManageMessageId.SetPasswordFailure ? "The current password is incorrect or the new password is invalid."

            : message == ManageMessageId.ResetPasswordSuccess ? "The password for this account password has been reset."
            : message == ManageMessageId.ResetPasswordFailure ? "The password for this account password has been reset."

            : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
            : message == ManageMessageId.AddLoginSuccess ? "External login was added."
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
            ChangePasswordMismatch,
            ChangePasswordWrongOriginal,
            ChangePasswordFailure,
            ResetPasswordSuccess,
            ResetPasswordFailure,
            SetPasswordSuccess,
            SetPasswordFailure,
            RemoveLoginSuccess,
            AddLoginSuccess
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

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
