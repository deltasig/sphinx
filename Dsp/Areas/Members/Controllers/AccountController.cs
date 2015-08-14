namespace Dsp.Areas.Members.Controllers
{
    using Dsp.Controllers;
    using Edu.Controllers;
    using Entities;
    using Extensions;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Models;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;

    [Authorize, RequireHttps]
    public class AccountController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index(string userName, 
            ManageMessageId? accountMessage, MajorsController.MajorsMessageId? majorMessage)
        {
            switch (accountMessage)
            {
                case ManageMessageId.ExternalLoginAddSuccess:
                case ManageMessageId.ChangePasswordSuccess:
                case ManageMessageId.ExternalLoginRemoveSuccess:
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
                case MajorsController.MajorsMessageId.AssignSuccess:
                case MajorsController.MajorsMessageId.UnassignSuccess:
                    ViewBag.SuccessMessage = MajorsController.GetResultMessage(majorMessage);
                    break;
            }

            if (string.IsNullOrEmpty(userName)) userName = User.Identity.GetUserName();

            var member = await UserManager.FindByNameAsync(userName);
            var thisSemester = await GetThisSemesterAsync();
            var model = new AccountInformationModel
            {
                Member = member,
                CurrentSemester = thisSemester,
                ThisSemesterCourses = member.ClassesTaken
                    .Where(c => c.SemesterId == thisSemester.SemesterId)
            };
            ViewBag.UserName = userName;

            return View(model);
        }
        
        public async Task<ActionResult> Manage(ManageMessageId? message, string userName)
        {
            ViewBag.FailMessage =
                message == ManageMessageId.ExternalLoginRemoveFailure ? "Failed to remove external login."
                : message == ManageMessageId.ExternalLoginInfoNotFound ? "External login info not found."
                : message == ManageMessageId.ExternalLoginAddFailure ? "Failed to link external login to local account."
                : message == ManageMessageId.ChangePasswordWrongOriginal ? GetManageMessage(message)
                : message == ManageMessageId.ChangePasswordMismatch ? GetManageMessage(message)
                : message == ManageMessageId.ChangePasswordMissingInfo ? GetManageMessage(message)
                : message == ManageMessageId.ChangePasswordFailure ? GetManageMessage(message)
                : message == ManageMessageId.ResetPasswordFailure ? GetManageMessage(message)
                : message == ManageMessageId.ResendConfirmationAlreadyConfirmed ? GetManageMessage(message)
                : "";
            ViewBag.SuccessMessage =
                message == ManageMessageId.ExternalLoginRemoveSuccess ? "The external login was successfully removed."
                : message == ManageMessageId.ExternalLoginAddSuccess ? "The external login was successfully added."
                : message == ManageMessageId.ChangePasswordSuccess ? GetManageMessage(message)
                : message == ManageMessageId.ResetPasswordSuccess ? GetManageMessage(message)
                : message == ManageMessageId.ResendConfirmationSuccess ? GetManageMessage(message)
                : "";

            if (string.IsNullOrEmpty(userName)) userName = User.Identity.GetUserName();

            var user = await UserManager.FindByNameAsync(userName);
            var userLogins = await UserManager.GetLoginsAsync(user.Id);
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes()
                .Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider))
                .ToList();

            ViewBag.UserName = userName;

            return View(new AccountManagementModel
            {
                Member = user,
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string userName, AccountChangeMessageId? message)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailMessage = TempData["FailureMessage"];

            if (string.IsNullOrEmpty(userName))
            {
                userName = User.Identity.GetUserName();
            }
            if (!User.IsInRole("Administrator") && !User.IsInRole("Secretary") && User.Identity.Name != userName)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
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
            if(!User.IsInRole("Administrator") && !User.IsInRole("Secretary") && User.Identity.Name != model.Member.UserName)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
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
            member.StatusId = model.Member.StatusId;
            member.PledgeClassId = model.Member.PledgeClassId;
            member.ExpectedGraduationId = model.Member.ExpectedGraduationId;
            member.BigBroId = model.Member.BigBroId == 0 ? null : model.Member.BigBroId;
            member.ShirtSize = model.Member.ShirtSize;
            member.LastUpdatedOn = DateTime.UtcNow;

            UserManager.Update(member);

            ViewBag.SuccessMessage = GetAccountChangeMessage(AccountChangeMessageId.UpdateSuccess);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Avatar(FormCollection formCollection)
        {
            var userId = int.Parse(formCollection["Member.Id"]);
            var member = await UserManager.FindByIdAsync(userId);
            var file = Request.Files[0];
            if (file == null || file.ContentLength <= 0)
            {
                TempData["FailureMessage"] = "Upload failure (no file received).";
                return RedirectToAction("Edit", new { userName = member.UserName });
            }
            if (file.ContentLength > 1000000)
            {
                TempData["FailureMessage"] = "Upload failure (file too large; max upload size is 1mb).";
                return RedirectToAction("Edit", new { userName = member.UserName });
            }

            var imageUpload = new ImageUpload { Width = 300, Height = 300 };
            var imageResult = imageUpload.RenameUploadFile(file, member.UserName);
            if (imageResult.Success)
            {
                member.AvatarPath = imageResult.ImageName;
                UserManager.Update(member);
                TempData["SuccessMessage"] = "Successfully uploaded new image.";
            }
            else
            {
                TempData["FailureMessage"] = imageResult.ErrorMessage;
            }

            return RedirectToAction("Edit", new { userName = member.UserName });
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
                ViewBag.FailMessage = "The information provided was invalid.";
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ViewBag.FailMessage = "The information provided was invalid.";
                return View(model);
            }
            if (!await UserManager.IsEmailConfirmedAsync(user.Id))
            {
                ViewBag.FailMessage = "The information provided was for an unconfirmed account.  " +
                                      "Please see the original registration email or email an administrator for another one.";
                return View(model);
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
                case SignInStatus.Failure:
                default:
                    ViewBag.FailMessage = "The information provided was either invalid or for an inactive account.";
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
        public async Task<ActionResult> Registration()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailMessage = TempData["FailureMessage"];

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
            if (!ModelState.IsValid)
            {
                TempData["FailureMessage"] = "Registration failed because the format of some of the provided data was invalid.";
                return RedirectToAction("Registration");
            }
            // Attempt to register the user
            try
            {
                var tempPassword = Membership.GeneratePassword(10, 5);
                var user = new Member
                {
                    UserName = model.UserName.ToLower(),
                    Email = model.Email.ToLower(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    StatusId = int.Parse(model.StatusId),
                    PledgeClassId = int.Parse(model.PledgeClassId),
                    ExpectedGraduationId = int.Parse(model.ExpectedGraduationId),
                    ShirtSize = model.ShirtSize,
                    CreatedOn = DateTime.UtcNow
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

                    // Send confirmation email.
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new
                    {
                        area = "Members",
                        userId = user.Id,
                        code = code
                    }, protocol: Request.Url.Scheme);

                    var confirmationModel = new RegistrationConfirmationModel
                    {
                        UserName = model.UserName,
                        TemporaryPassword = tempPassword,
                        CallbackUrl = callbackUrl
                    };
                    var body = RenderRazorViewToString("~/Views/Emails/RegistrationConfirmation.cshtml", confirmationModel);
                    await UserManager.SendEmailAsync(user.Id, "Account Registration Successful!", body);

                    TempData["SuccessMessage"] = user + " was successfully registered and emailed for confirmation.";
                    return RedirectToAction("Registration");
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
            return RedirectToAction("Registration");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(int userId, string code)
        {
            if (code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Unregister(UnregisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to remove the user from the system
                try
                {
                    var member = await UserManager.FindByIdAsync(model.SelectedUserId);

                    // Disallow unregistration if someone has meaninfully interacted with the system.
                    if(member.ClassesTaken.Any() || 
                       member.IncidentReports.Any() ||
                       member.LaundrySignups.Any() || 
                       member.LittleBrothers.Any() || 
                       member.MajorsToMember.Any() || 
                       member.Leaders.Any() ||
                       member.ClassFileUploads.Any() ||
                       member.Rooms.Any() ||
                       member.SoberSignups.Any() ||
                       member.ServiceHours.Any() ||
                       member.WorkOrders.Any())
                    {
                        TempData["FailureMessage"] = "Could not remove user because they have pertinent data that can't be removed.";
                        return RedirectToAction("Registration");
                    }

                    await UserManager.DeleteAsync(member);
                    
                    TempData["SuccessMessage"] = "Successfully removed the user.";
                    return RedirectToAction("Registration");
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
            TempData["FailureMessage"] = "Failed to remove the user for an unknown reason.  Please contact your administrator.";
            return RedirectToAction("Registration");
        }
        
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(AccountManagementModel model)
        {
            if (model.OldPassword == null || model.NewPassword == null || model.ConfirmPassword == null)
                return RedirectToAction("Manage", new { message = ManageMessageId.ChangePasswordMissingInfo });
            if (model.NewPassword != model.ConfirmPassword) 
                return RedirectToAction("Manage", new { message = ManageMessageId.ChangePasswordMismatch });

            var userId = User.Identity.GetUserId<int>();
            var result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);

            return RedirectToAction("Manage", result.Succeeded
                ? new { message = ManageMessageId.ChangePasswordSuccess }
                : new { message = ManageMessageId.ChangePasswordWrongOriginal });
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

                var emailService = new EmailService();
                await emailService.SendAsync(emailMessage);

                return RedirectToAction("Manage", new { member.UserName, message = ManageMessageId.ResetPasswordSuccess });
            }
            AddErrors(result);

            return RedirectToAction("Manage", new { member.UserName, message = ManageMessageId.ResetPasswordFailure });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> ResendConfirmationEmail(int? id)
        {
            if (id == null) return HttpNotFound();
            var member = await UserManager.FindByIdAsync((int)id);
            return View(member);
        }

        [Authorize(Roles = "Administrator"), HttpPost, ValidateAntiForgeryToken, ActionName("ResendConfirmationEmail")]
        public async Task<ActionResult> ResendConfirmationEmail(Member member)
        {
            if (await UserManager.IsEmailConfirmedAsync(member.Id))
            {
                return RedirectToAction("Manage", new { message = ManageMessageId.ResendConfirmationAlreadyConfirmed, member.UserName });
            }

            var tempPassword = Membership.GeneratePassword(10, 0);
            var token = await UserManager.GeneratePasswordResetTokenAsync(member.Id);
            var result = await UserManager.ResetPasswordAsync(member.Id, token, tempPassword);
            if (result.Succeeded)
            {
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(member.Id);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new
                {
                    area = "Members",
                    userId = member.Id,
                    code = code
                }, protocol: Request.Url.Scheme);

                var confirmationModel = new RegistrationConfirmationModel
                {
                    UserName = member.UserName,
                    TemporaryPassword = tempPassword,
                    CallbackUrl = callbackUrl
                };
                var body = RenderRazorViewToString("~/Views/Emails/RegistrationConfirmation.cshtml", confirmationModel);
                await UserManager.SendEmailAsync(member.Id, "Confirm your account!", body);

                return RedirectToAction("Manage", new { message = ManageMessageId.ResendConfirmationSuccess, member.UserName });
            }
            AddErrors(result);

            return RedirectToAction("Manage", new { message = ManageMessageId.ResetPasswordFailure, member.UserName });
        }

        #region External Authentication

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("SignIn");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then they can't sign in.
                    return RedirectToAction("ExternalLoginFailure");
            }
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "Sphinx" });
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new Member { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.ExternalLoginInfoNotFound });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId<int>(), loginInfo.Login);
            return result.Succeeded
                ? RedirectToAction("Manage", new { Message = ManageMessageId.ExternalLoginAddSuccess })
                : RedirectToAction("Manage", new { Message = ManageMessageId.ExternalLoginAddFailure });
        }
        
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId<int>(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.ExternalLoginRemoveSuccess;
            }
            else
            {
                message = ManageMessageId.ExternalLoginRemoveFailure;
            }
            return RedirectToAction("Manage", new { Message = message });
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
            return message == ManageMessageId.ChangePasswordSuccess ? "The password change for this account was successful."
            : message == ManageMessageId.ChangePasswordWrongOriginal ? "The password change failed because the wrong original password was entered."
            : message == ManageMessageId.ChangePasswordMismatch ? "The password change failed because the new password fields did not match."
            : message == ManageMessageId.ChangePasswordMissingInfo ? "The password change failed because one of the fields did not contain any information."
            : message == ManageMessageId.ChangePasswordFailure ? "The password change failed for an unknown reason.  Please contact your administrator."

            : message == ManageMessageId.SetPasswordSuccess ? "The password for this account password has been set."
            : message == ManageMessageId.SetPasswordFailure ? "The current password is incorrect or the new password is invalid."

            : message == ManageMessageId.ResetPasswordSuccess ? "The password for this account has been reset."
            : message == ManageMessageId.ResetPasswordFailure ? "The password for this account could not be reset for some unknown reason."
            : message == ManageMessageId.ResendConfirmationSuccess ? "The confirmation email for this account was sent and the password was reset."
            : message == ManageMessageId.ResendConfirmationAlreadyConfirmed ? "This account is already confirmed.  Try just resetting their passwored."

            : message == ManageMessageId.ExternalLoginRemoveSuccess ? "The external login was removed."
            : message == ManageMessageId.ExternalLoginAddSuccess ? "External login was added."
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
            ExternalLoginRemoveSuccess,
            ExternalLoginRemoveFailure,
            ExternalLoginAddSuccess,
            ExternalLoginInfoNotFound,
            ExternalLoginAddFailure,
            ChangePasswordMissingInfo,
            ResendConfirmationSuccess,
            ResendConfirmationAlreadyConfirmed
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
            return RedirectToAction("Index", "Home", new { area = "Sphinx" });
        }

        private const string XsrfKey = "XsrfId";

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        public class ImageResult
        {
            public bool Success { get; set; }
            public string ImageName { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class ImageUpload
        {
            public int Width { get; set; }
            public int Height { get; set; }
            private const string UploadPath = "~/Images/Avatars/";

            public ImageResult RenameUploadFile(HttpPostedFileBase file, string userName)
            {
                var imageResult = new ImageResult { Success = true, ErrorMessage = null };
                var fileExtension = Path.GetExtension(file.FileName);
                var finalFileName = Regex.Replace(userName, @"\s+", "") + fileExtension;
                var folderPath = System.Web.HttpContext.Current.Server.MapPath(UploadPath);
                var imagePath = System.Web.HttpContext.Current.Server.MapPath(Path.Combine(UploadPath, finalFileName));

                // Create image directory if it does not exist.
                var directoryExists = Directory.Exists(folderPath);
                if (!directoryExists)
                {
                    Directory.CreateDirectory(folderPath);
                }
                // Validate extension.
                if (!ValidateExtension(fileExtension))
                {
                    imageResult.Success = false;
                    imageResult.ErrorMessage = "Invalid Extension.";
                    return imageResult;
                }
                // Delete old file if one exists.
                var fileExists = System.IO.File.Exists(imagePath);
                if (fileExists)
                {
                    System.IO.File.Delete(imagePath);
                }

                return UploadFile(file, finalFileName, imageResult);
            }

            private ImageResult UploadFile(HttpPostedFileBase file, string fileName, ImageResult imageResult)
            {
                var path = System.Web.HttpContext.Current.Server.MapPath(Path.Combine(UploadPath, fileName));

                try
                {
                    file.SaveAs(path);
                    var imgOriginal = Image.FromFile(path);
                    var imgActual = Scale(imgOriginal);
                    imgOriginal.Dispose();
                    imgActual.Save(path);
                    imgActual.Dispose();

                    imageResult.ImageName = fileName;
                    return imageResult;
                }
                catch (Exception ex)
                {
                    // you might NOT want to show the exception error for the user
                    // this is generally logging or testing
                    imageResult.Success = false;
                    imageResult.ErrorMessage = ex.Message;
                    return imageResult;
                }
            }

            private bool ValidateExtension(string extension)
            {
                extension = extension.ToLower();
                switch (extension)
                {
                    case ".jpg":
                        return true;
                    case ".png":
                        return true;
                    case ".jpeg":
                        return true;
                    default:
                        return false;
                }
            }

            private Image Scale(Image imgPhoto)
            {
                float sourceWidth = imgPhoto.Width;
                float sourceHeight = imgPhoto.Height;
                float destHeight = 0;
                float destWidth = 0;
                var sourceX = 0;
                var sourceY = 0;
                var destX = 0;
                var destY = 0;

                // For resize, possible distortion
                if (Width != 0 && Height != 0)
                {
                    destWidth = Width;
                    destHeight = Height;
                }
                // Change size proportionally
                else if (Height != 0)
                {
                    destWidth = (float)(Height * sourceWidth) / sourceHeight;
                    destHeight = Height;
                }
                else
                {
                    destWidth = Width;
                    destHeight = (float)(sourceHeight * Width / sourceWidth);
                }

                var bmPhoto = new Bitmap((int)destWidth, (int)destHeight, PixelFormat.Format32bppPArgb);
                bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

                var grPhoto = Graphics.FromImage(bmPhoto);
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

                grPhoto.DrawImage(imgPhoto,
                    new Rectangle(destX, destY, (int)destWidth, (int)destHeight),
                    new Rectangle(sourceX, sourceY, (int)sourceWidth, (int)sourceHeight),
                    GraphicsUnit.Pixel);

                grPhoto.Dispose();

                return bmPhoto;
            }
        }
    }
}
