namespace Dsp.Web.Areas.Members.Controllers
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Repositories;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Dsp.Web.Controllers;
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
        private readonly IPositionService _positionService;

        public AccountController()
        {
            var repo = new Repository<SphinxDbContext>(_db);
            _positionService = new PositionService(repo);
        }

        public AccountController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet]
        public async Task<ActionResult> Index(string userName, ManageMessageId? accountMessage)
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
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.FailureMessage = TempData["FailureMessage"];

            if (string.IsNullOrEmpty(userName)) userName = User.Identity.GetUserName();

            var member = await UserManager.FindByNameAsync(userName);
            var thisSemester = await GetThisSemesterAsync();
            var model = new AccountInformationModel
            {
                Member = member,
                CurrentSemester = thisSemester,
                ThisSemesterCourses = member.ClassesTaken
                    .Where(c => c.SemesterId == thisSemester.Id)
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

            var userId = User.Identity.GetUserId<int>();
            var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Secretary");
            if (!hasElevatedPermissions && User.Identity.Name != userName)
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
            var userId = User.Identity.GetUserId<int>();
            var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Secretary");
            if (!hasElevatedPermissions && User.Identity.Name != model.Member.UserName)
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
            member.DietaryInstructions = model.Member.DietaryInstructions;
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
                        return RedirectToAction("Sphinx", "Home", new { area = "" });
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
                    PledgeClassList = await GetPledgeClassListWithNoneAsync(),
                    SemesterList = await GetAllSemesterListWithNoneAsync()
                },
                UnregisterModel = new UnregisterModel
                {
                    Users = await GetUsersAsFullNameAsync(
                        u =>
                            u.MemberStatus.StatusName == "New" ||
                            u.MemberStatus.StatusName == "Affiliate",
                        u =>
                            u.CreatedOn != null &&
                            (DateTime.UtcNow - u.CreatedOn.Value).TotalDays < 30)
                }
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator, Secretary, Director of Recruitment, New Member Education")]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("Registration failed because the format of some of the provided data was invalid.");
                }
                var existingUser = await UserManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    throw new ArgumentException("This email is already in use, please select another.");
                }

                var tempPassword = Membership.GeneratePassword(10, 5);
                var pledgeClassId = int.Parse(model.PledgeClassId);
                var expectedGraduationId = int.Parse(model.ExpectedGraduationId);
                var user = new Member
                {
                    UserName = model.UserName.ToLower(),
                    Email = model.Email.ToLower(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    StatusId = int.Parse(model.StatusId),
                    CreatedOn = DateTime.UtcNow
                };
                if (pledgeClassId > 0) user.PledgeClassId = pledgeClassId;
                if (expectedGraduationId > 0) user.ExpectedGraduationId = expectedGraduationId;

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
                    await SendConfirmationEmailAsync(user.Id, model.UserName, tempPassword);

                    TempData["SuccessMessage"] = user + " was successfully registered and emailed for confirmation.";
                }
                else
                {
                    var msg = "Something unexpected went wrong.  Please contact your administrator.";
                    if (result.Errors.Any())
                    {
                        msg = string.Join(" ", result.Errors);
                    }
                    throw new Exception(msg);
                }
            }
            catch (ArgumentException e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            catch (MembershipCreateUserException e)
            {
                TempData["FailureMessage"] = ErrorCodeToString(e.StatusCode);
            }
            catch (Exception e)
            {
                TempData["FailureMessage"] = e.Message;
            }
            return RedirectToAction("Registration");
        }

        private async Task SendConfirmationEmailAsync(int userId, string username, string tempPassword)
        {
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(userId);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new
            {
                area = "Members",
                userId = userId,
                code
            }, protocol: Request.Url.Scheme);

            var confirmationModel = new RegistrationConfirmationModel
            {
                UserName = username,
                TemporaryPassword = tempPassword,
                CallbackUrl = callbackUrl
            };
            var body = RenderRazorViewToString("~/Views/Emails/RegistrationConfirmation.cshtml", confirmationModel);
            await UserManager.SendEmailAsync(userId, "Account Registration Successful!", body);
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

        [AllowAnonymous]
        public ActionResult ResendConfirmationEmail()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResendConfirmationEmail(ResendConfirmationEmailModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ResendConfirmationEmailConfirmation");
                }

                // Resend email
                var tempPassword = Membership.GeneratePassword(10, 5);
                var resetToken = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                await UserManager.ResetPasswordAsync(user.Id, resetToken, tempPassword);
                await SendConfirmationEmailAsync(user.Id, user.UserName, tempPassword);
                return RedirectToAction("ResendConfirmationEmailConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ResendConfirmationEmailConfirmation()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Unregister(UnregisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to remove the user from the system
                try
                {
                    var user = await UserManager.FindByIdAsync(model.SelectedUserId);

                    // Disallow unregistration if someone has meaninfully interacted with the system.
                    var userInteractedWithSystem = false;
                    var interactionMessage = "";
                    if (user.ClassesTaken.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Classes";
                    }
                    else if (user.BigBroId != null)
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Big Brother";
                    }
                    else if (user.LittleBrothers.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Little Brothers";
                    }
                    else if (user.LaundrySignups.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Laundry Signups";
                    }
                    else if (user.ServiceHours.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Service Hours";
                    }
                    else if (user.SoberSignups.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Sober Signups";
                    }
                    else if (user.PositionsHeld.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Positions";
                    }
                    else if (user.IncidentReports.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Incident Reports";
                    }
                    else if (user.MealPlates.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Meal Plates";
                    }
                    else if (user.Rooms.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Rooms";
                    }
                    else if (user.MealItemVotes.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Meal Item Votes";
                    }
                    else if (user.ServiceHourAmendments.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Service Hour Amendments";
                    }
                    else if (user.ServiceEventAmendments.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Service Event Amendments";
                    }
                    else if (user.SubmittedEvents.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Event Submission";
                    }
                    else if (user.WorkOrders.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Work Orders";
                    }
                    else if (user.WorkOrderComments.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Work Order Comments";
                    }
                    else if (user.WorkOrderPriorityChanges.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Work Order Priority Changes";
                    }
                    else if (user.WorkOrderStatusChanges.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Work Order Status Changes";
                    }
                    else if (user.ChoreGroups.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Chore Groups";
                    }
                    else if (user.ChoreSignOffs.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Chore Sign Offs";
                    }
                    else if (user.ChoresEnforced.Any())
                    {
                        userInteractedWithSystem = true;
                        interactionMessage = "Chores Enforced";
                    }

                    if (!userInteractedWithSystem)
                    {
                        await UserManager.DeleteAsync(user);
                        TempData["SuccessMessage"] = "Successfully removed the user.";
                    }
                    else
                    {
                        TempData["FailureMessage"] = "Could not remove user because they have interacted with the system. " +
                            $"Interaction: {interactionMessage}";
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    TempData["FailureMessage"] = $"Error, please contact your administrator. {e.Message}";
                }
                catch (InvalidOperationException e)
                {
                    TempData["FailureMessage"] = $"Error, please contact your administrator. {e.Message}";
                }
                catch (Exception e)
                {
                    TempData["FailureMessage"] = $"Error, please contact your administrator. {e.Message}";
                }
            }
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

        [AllowAnonymous]
        public ActionResult ForgotUsername()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotUsername(ForgotUsernameModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotUsernameConfirmation");
                }

                await UserManager.SendEmailAsync(user.Id, "Sphinx Username", "Your username is: " + user.UserName);
                return RedirectToAction("ForgotUsernameConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotUsernameConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // Send email with link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Sphinx Password Reset", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
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
                return RedirectToAction("Sphinx", "Home", new { area = "" });
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
            return RedirectToAction("Sphinx", "Home", new { area = "" });
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

            public static string GetUploadPath(string fileName)
            {
                if (fileName == null)
                {
                    fileName = string.Empty;
                }
                return System.Web.HttpContext.Current.Server.MapPath(Path.Combine(UploadPath, fileName));
            }

            public ImageResult RenameUploadFile(HttpPostedFileBase file, string userName)
            {
                var imageResult = new ImageResult { Success = true, ErrorMessage = null };
                var fileExtension = Path.GetExtension(file.FileName);
                var finalFileName = Regex.Replace(userName, @"\s+", "") + fileExtension;
                var folderPath = System.Web.HttpContext.Current.Server.MapPath(UploadPath);
                var imagePath = GetUploadPath(finalFileName);

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
