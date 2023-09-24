namespace Dsp.WebCore.Areas.Members.Controllers;

using Dsp.Data.Entities;
using Dsp.Services.Interfaces;
using Dsp.WebCore.Controllers;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

[Authorize, RequireHttps]
public class AccountController : BaseController
{
    private readonly IWebHostEnvironment _env;
    private readonly IPositionService _positionService;

    public AccountController(IWebHostEnvironment env, IPositionService positionService)
    {
        _env = env;
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

        if (string.IsNullOrEmpty(userName)) userName = User.GetUserName();

        var member = await UserManager.FindByNameAsync(userName);
        var thisSemester = await GetThisSemesterAsync();
        var model = new AccountInformationModel
        {
            User = member,
            CurrentSemester = thisSemester,
            ThisSemesterCourses = member.ClassesTaken
                .Where(c => c.SemesterId == thisSemester.Id),
            Roles = await UserManager.GetRolesAsync(member)
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

        if (string.IsNullOrEmpty(userName)) userName = User.GetUserName();

        var user = await UserManager.FindByNameAsync(userName);

        ViewBag.UserName = userName;

        return View(new AccountManagementModel
        {
            User = user,
        });
    }

    [HttpGet]
    public async Task<ActionResult> Edit(string userName, AccountChangeMessageId? message)
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.FailMessage = TempData["FailureMessage"];

        if (string.IsNullOrEmpty(userName))
        {
            userName = User.GetUserName();
        }

        var userId = User.GetUserId();
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Secretary");
        if (!hasElevatedPermissions && User.Identity.Name != userName)
        {
            return new StatusCodeResult((int) HttpStatusCode.NotFound);
        }
        var member = string.IsNullOrEmpty(userName)
            ? await UserManager.FindByNameAsync(User.Identity.Name)
            : await UserManager.FindByNameAsync(userName);
        var model = new EditMemberInfoModel
        {
            User = member,
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
        var userId = User.GetUserId();
        var hasElevatedPermissions = await _positionService.UserHasPositionPowerAsync(userId, "Secretary");
        if (!hasElevatedPermissions && User.Identity.Name != model.User.UserName)
        {
            return new StatusCodeResult((int) HttpStatusCode.NotFound);
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

        var member = await UserManager.FindByIdAsync(model.User.Id.ToString());
        member.FirstName = model.User.FirstName;
        member.LastName = model.User.LastName;
        member.Email = model.User.Email;
        member.StatusId = model.User.StatusId;
        member.PledgeClassId = model.User.PledgeClassId;
        member.ExpectedGraduationId = model.User.ExpectedGraduationId;
        member.BigBroId = model.User.BigBroId == 0 ? null : model.User.BigBroId;
        member.DietaryInstructions = model.User.DietaryInstructions;
        member.LastUpdatedOn = DateTime.UtcNow;

        Context.Update(member);
        await Context.SaveChangesAsync();

        ViewBag.SuccessMessage = GetAccountChangeMessage(AccountChangeMessageId.UpdateSuccess);

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Avatar(FormCollection formCollection)
    {
        var userId = formCollection["Member.Id"];
        var member = await UserManager.FindByIdAsync(userId);
        var file = Request.Form.Files[0];
        if (file == null || file.Length <= 0)
        {
            TempData["FailureMessage"] = "Upload failure (no file received).";
            return RedirectToAction("Edit", new { userName = member.UserName });
        }
        if (file.Length > 1000000)
        {
            TempData["FailureMessage"] = "Upload failure (file too large; max upload size is 1mb).";
            return RedirectToAction("Edit", new { userName = member.UserName });
        }

        var imageUpload = new ImageUpload (_env) { Width = 300, Height = 300 };
        var imageResult = await imageUpload.RenameUploadFileAsync(file, member.UserName);
        if (imageResult.Success)
        {
            member.AvatarPath = imageResult.ImageName;
            Context.Update(User);
            await Context.SaveChangesAsync();
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
        return View(new SignInModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<ActionResult> SignIn(SignInModel model)
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
        if (!await UserManager.IsEmailConfirmedAsync(user))
        {
            ViewBag.FailMessage = "The information provided was for an unconfirmed account.  " +
                                  "Please see the original registration email or email an administrator for another one.";
            return View(model);
        }

        var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true);
        if (result.Succeeded)
        {
            return RedirectToAction("Sphinx", "Home", new { area = "" });
        }
        else if(result.IsNotAllowed)
        {
            return View("Lockout");
        }
        else
        {
            ViewBag.FailMessage = "The information provided was either invalid or for an inactive account.";
            return View(model);
        }
    }

    [HttpGet]
    public async Task<ActionResult> SignOut()
    {
        await SignInManager.SignOutAsync();
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
                        u.Status.StatusName == "New" ||
                        u.Status.StatusName == "Affiliate",
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

            var tempPassword = PasswordHelper.GenerateRandomPassword();
            var pledgeClassId = int.Parse(model.PledgeClassId);
            var expectedGraduationId = int.Parse(model.ExpectedGraduationId);
            var user = new User
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

                await Context.SaveChangesAsync();

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
        catch (Exception e)
        {
            TempData["FailureMessage"] = e.Message;
        }
        return RedirectToAction("Registration");
    }

    private async Task SendConfirmationEmailAsync(int userId, string username, string tempPassword)
    {
        var user = await UserManager.GetUserAsync(User);
        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = Url.Action("ConfirmEmail", "Account", new
        {
            area = "Members",
            userId = userId,
            code
        }, protocol: Request.Scheme);

        var confirmationModel = new RegistrationConfirmationModel
        {
            UserName = username,
            TemporaryPassword = tempPassword,
            CallbackUrl = callbackUrl
        };
        var body = RenderRazorViewToString("~/Views/Emails/RegistrationConfirmation.cshtml", confirmationModel);
        // TODO: await UserManager.SendEmailAsync(userId, "Account Registration Successful!", body);
    }

    [AllowAnonymous]
    public async Task<ActionResult> ConfirmEmail(int userId, string code)
    {
        if (code == null)
        {
            return View("Error");
        }
        var user = await UserManager.GetUserAsync(User);
        var result = await UserManager.ConfirmEmailAsync(user, code);
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
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ResendConfirmationEmailConfirmation");
            }

            // Resend email
            var tempPassword = PasswordHelper.GenerateRandomPassword();
            var resetToken = await UserManager.GeneratePasswordResetTokenAsync(user);
            await UserManager.ResetPasswordAsync(user, resetToken, tempPassword);
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
                var user = await UserManager.FindByIdAsync(model.SelectedUserId.ToString());

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
                else if (user.LittleBros.Any())
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
                else if (user.ServiceEvents.Any())
                {
                    userInteractedWithSystem = true;
                    interactionMessage = "Event Submission";
                }
                else if (user.WorkOrders.Any())
                {
                    userInteractedWithSystem = true;
                    interactionMessage = "Work Orders";
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

        var user = await UserManager.GetUserAsync(User);
        var result = await UserManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

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
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotUsernameConfirmation");
            }

            // TODO: await UserManager.SendEmailAsync(user.Id, "Sphinx Username", "Your username is: " + user.UserName);
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
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            // Send email with link
            string code = await UserManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
            // TODO: await UserManager.SendEmailAsync(user.Id, "Sphinx Password Reset", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
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
        var result = await UserManager.ResetPasswordAsync(user, model.Code, model.Password);
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

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
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
        private readonly IWebHostEnvironment _env;

        public ImageUpload(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GetUploadPath(string fileName)
        {
            if (fileName == null)
            {
                fileName = string.Empty;
            }
            var path = Path.Combine(
                Path.Combine(_env.WebRootPath, UploadPath),
                fileName
            );
            return path;
        }

        public async Task<ImageResult> RenameUploadFileAsync(IFormFile file, string userName)
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

            return await UploadFileAsync(file, finalFileName, imageResult);
        }

        private async Task<ImageResult> UploadFileAsync(IFormFile file, string fileName, ImageResult imageResult)
        {
            var path = GetUploadPath(fileName);
            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
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
