namespace Dsp.Areas.Edu.Controllers
{
    using Amazon.S3;
    using Amazon.S3.Model;
    using Entities;
    using global::Dsp.Controllers;
    using Microsoft.AspNet.Identity;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using System.Web.Mvc;

    [Authorize(Roles = "Pledge, Neophyte, Active, Administrator")]
    public class ClassesController : BaseController
    {
        public async Task<ActionResult> Index(ClassesIndexFilterModel filter, ClassMessageId? message)
        {
            switch (message)
            {
                case ClassMessageId.DeleteClassFailure:
                case ClassMessageId.EditClassFailure:
                    ViewBag.FailMessage = GetClassResultMessage(message);
                    break;
                case ClassMessageId.DeleteClassSuccess:
                case ClassMessageId.EditClassSuccess:
                    ViewBag.SuccessMessage = GetClassResultMessage(message);
                    break;
            }
            var classes = await _db.Classes.ToListAsync();

            const int pageSize = 10;
            var filterResults = await base.GetFilteredClassList(
                classes, filter.s, filter.sort, filter.page, filter.select, pageSize);

            var model = new ClassIndexModel
            {
                Classes = filterResults, 
                CurrentSemester = await GetThisSemesterAsync()
            };
            return View(model);
        }

        public async Task<ActionResult> Create()
        {
            var model = new CreateClassModel();
            model.Departments = new SelectList(await 
                _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name");
            model.Class = new Class();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateClassModel model)
        {
            var classExistsAlready = (await _db.Classes.AnyAsync(c =>
                c.CourseShorthand == model.Class.CourseShorthand && c.DepartmentId == model.Class.DepartmentId));
            
            if (ModelState.IsValid && !classExistsAlready)
            {
                _db.Classes.Add(model.Class);
                await _db.SaveChangesAsync();
                ViewBag.SuccessMessage = GetClassResultMessage(ClassMessageId.AddClassSuccess);
                model.Class = new Class();
                model.Departments = new SelectList(await 
                    _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name");
            }
            else
            {
                ViewBag.FailMessage = GetClassResultMessage(ClassMessageId.AddClassFailure);
                model.Departments = new SelectList(await 
                    _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name", model.Class.DepartmentId);
            }
            return View(model);
        }

        public async Task<ActionResult> Details(int? id, ClassFileMessageId? message)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var course = await _db.Classes.FindAsync(id);
            if (course == null)
            {
                return HttpNotFound();
            }

            switch (message)
            {
                case ClassFileMessageId.UploadInvalidFailure:
                case ClassFileMessageId.UploadInvalidFileTypeFailure:
                case ClassFileMessageId.DownloadFileAwsFailure:
                case ClassFileMessageId.DeleteFileAwsFailure:
                    ViewBag.FailMessage = GetFileResultMessage(message);
                    break;
                case ClassFileMessageId.UploadFileSuccess:
                case ClassFileMessageId.DeleteFileSuccess:
                    ViewBag.SuccessMessage = GetFileResultMessage(message);
                    break;
            }

            var model = new ClassDetailsModel
            {
                Class = course,
                CurrentSemester = await GetThisSemesterAsync(),
                FileInfoModel = new FileInfoModel
                {
                    ExistingFiles = new List<string> { "tests.pdf", "homework.pdf", "notes.pdf" }
                }
            };
            return View(model);
        }

        public async Task<ActionResult> Upvote(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var file = await _db.ClassFiles.FindAsync(id);
            if (file == null)
            {
                return HttpNotFound();
            }
            var userId = User.Identity.GetUserId<int>();
            var existingVote = await _db.ClassFileVotes
                    .SingleOrDefaultAsync(v =>
                        v.UserId == userId &&
                        v.ClassFileId == file.ClassFileId);

            if (existingVote == null)
            {
                _db.ClassFileVotes.Add(new ClassFileVote
                {
                    UserId = userId,
                    ClassFileId = file.ClassFileId,
                    IsUpvote = true
                });
            }
            else
            {
                if (existingVote.IsUpvote)
                {
                    _db.Entry(existingVote).State = EntityState.Deleted;
                }
                else
                {
                    existingVote.IsUpvote = true;
                    _db.Entry(existingVote).State = EntityState.Modified;
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = file.ClassId });
        }

        public async Task<ActionResult> Downvote(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var file = await _db.ClassFiles.FindAsync(id);
            if (file == null)
            {
                return HttpNotFound();
            }
            var userId = User.Identity.GetUserId<int>();
            var existingVote = await _db.ClassFileVotes
                    .SingleOrDefaultAsync(v =>
                        v.UserId == userId &&
                        v.ClassFileId == file.ClassFileId);

            if (existingVote == null)
            {
                _db.ClassFileVotes.Add(new ClassFileVote
                {
                    UserId = userId,
                    ClassFileId = file.ClassFileId,
                    IsUpvote = false
                });
            }
            else
            {
                if (!existingVote.IsUpvote)
                {
                    _db.Entry(existingVote).State = EntityState.Deleted;
                }
                else
                {
                    existingVote.IsUpvote = false;
                    _db.Entry(existingVote).State = EntityState.Modified;
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = file.ClassId });
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            var model = new CreateClassModel {Class = await _db.Classes.FindAsync(id)};
            if (model.Class == null)
            {
                return HttpNotFound();
            }
            model.Departments = new SelectList(await
                _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name");
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CreateClassModel model)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(model.Class).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { message = ClassMessageId.EditClassSuccess });
            }
            ViewBag.FailMessage = GetClassResultMessage(ClassMessageId.EditClassFailure);
            model.Departments = new SelectList(await
                    _db.Departments.OrderBy(c => c.Name).ToListAsync(), "DepartmentId", "Name", model.Class.DepartmentId);
            return View(model);
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var @class = await _db.Classes.FindAsync(id);
            if (@class == null)
            {
                return HttpNotFound();
            }

            if (@class.ClassesTaken.Any())
            {
                return RedirectToAction("Index", new
                {
                    message = ClassMessageId.DeleteClassFailure
                });
            }

            return View(@class);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var @class = await _db.Classes.FindAsync(id);
            if (@class.ClassesTaken.Any())
            {
                return RedirectToAction("Index", new
                {
                    message = ClassMessageId.DeleteClassFailure
                });
            }
            _db.Entry(@class).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new
            {
                message = ClassMessageId.DeleteClassSuccess
            });
        }
        
        public async Task<ActionResult> Schedule(string userName, ClassEnrollmentMessageId? message)
        {
            ViewBag.Message = string.Empty;

            if (string.IsNullOrEmpty(userName))
            {
                ViewBag.FailMessage = "No one by the username was found!";
                return View(new List<ClassTaken>());
            }

            switch (message)
            {
                case ClassEnrollmentMessageId.AddClassTakenDuplicateFailure:
                case ClassEnrollmentMessageId.AddClassTakenUnknownFailure:
                case ClassEnrollmentMessageId.DeleteClassTakenFailure:
                case ClassEnrollmentMessageId.EditClassTakenFailure:
                    ViewBag.FailMessage = GetEnrollmentResultMessage(message);
                    break;
                case ClassEnrollmentMessageId.AddClassTakenSuccess:
                case ClassEnrollmentMessageId.DeleteClassTakenSuccess:
                case ClassEnrollmentMessageId.EditClassTakenSuccess:
                    ViewBag.SuccessMessage = GetEnrollmentResultMessage(message);
                    break;
            }

            var member = await UserManager.FindByNameAsync(userName);

            var model = new ClassScheduleModel
            {
                SelectedUserName = userName,
                Member = member,
                AllClasses = await _db.Classes.OrderBy(c => c.CourseShorthand).ToListAsync(),
                Semesters = await GetSemesterListAsync(),
                ClassTaken = new ClassTaken
                {
                    SemesterId = (await GetThisSemesterAsync()).SemesterId,
                    UserId = member.Id
                },
                ClassesTaken = member.ClassesTaken.OrderByDescending(s => s.Semester.DateStart)
            };

            if(!model.ClassesTaken.Any())
            {
                ViewBag.Message += "No classes found for " + userName + ". ";
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddClassTaken(ClassScheduleModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Schedule", new
                {
                    userName = model.SelectedUserName, 
                    message = ClassEnrollmentMessageId.AddClassTakenUnknownFailure
                });

            if (!User.IsInRole("Academics") && !User.IsInRole("Administrator") &&
                User.Identity.GetUserName() != model.SelectedUserName)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var member = await UserManager.FindByNameAsync(model.SelectedUserName);

            if (member.ClassesTaken.Any(c => 
                c.ClassId == model.ClassTaken.ClassId && 
                c.UserId == model.ClassTaken.UserId && 
                c.SemesterId == model.ClassTaken.SemesterId))
            {
                return RedirectToAction("Schedule", new
                {
                    userName = model.SelectedUserName,
                    message = ClassEnrollmentMessageId.AddClassTakenDuplicateFailure
                });
            }

            _db.ClassesTaken.Add(model.ClassTaken);
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule", new
            {
                userName = model.SelectedUserName, 
                message = ClassEnrollmentMessageId.AddClassTakenSuccess
            });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteFromSchedule(int id, int sid, int cid, string username)
        {
            var entry = await _db.ClassesTaken.SingleAsync(c => c.UserId == id && c.SemesterId == sid && c.ClassId == cid);
            if (entry == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            return View(entry);
        }

        [HttpPost, ActionName("DeleteFromSchedule"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteConfirmed(ClassTaken model)
        {
            var entry = await _db.ClassesTaken.SingleAsync(c => c.UserId == model.UserId && c.SemesterId == model.SemesterId && c.ClassId == model.ClassId);
            var member = await UserManager.FindByIdAsync(model.UserId);
            if (entry == null)
            {
                return RedirectToAction("Schedule", new
                {
                    userName = member.UserName ?? User.Identity.GetUserName(),
                    message = ClassEnrollmentMessageId.DeleteClassTakenFailure
                });
            }

            _db.Entry(entry).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            return RedirectToAction("Schedule", new
            {
                userName = member.UserName ?? User.Identity.GetUserName(), 
                message = ClassEnrollmentMessageId.DeleteClassTakenSuccess
            });
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditClassTaken(int id, int sid, int cid)
        {
            var model = await _db.ClassesTaken.SingleAsync(c => c.UserId == id && c.SemesterId == sid && c.ClassId == cid);
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> EditClassTaken(ClassTaken classTaken)
        {
            if (!ModelState.IsValid) 
                return RedirectToAction("Schedule", new
                {
                    userName = classTaken.Member.UserName,
                    message = ClassEnrollmentMessageId.EditClassTakenFailure
                });

            _db.Entry(classTaken).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            var member = await UserManager.FindByIdAsync(classTaken.UserId);

            return RedirectToAction("Schedule", new { 
                userName = member.UserName,
                message = ClassEnrollmentMessageId.EditClassTakenSuccess 
            });
        }

        public async Task<ActionResult> Transcript(string userName, ClassEnrollmentMessageId? message)
        {
            if (!User.IsInRole("Administrator") && !User.IsInRole("Academics") && User.Identity.GetUserName() != userName)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            
            switch(message)
            {
                case ClassEnrollmentMessageId.UpdateTranscriptFailure:
                    ViewBag.FailMessage = GetEnrollmentResultMessage(message);
                    break;
                case ClassEnrollmentMessageId.UpdateTranscriptSuccess:
                    ViewBag.SuccessMessage = GetEnrollmentResultMessage(message);
                    break;
            }

            var member = await UserManager.Users.SingleAsync(m => m.UserName == userName);
            var model = new List<ClassTranscriptModel>();
            foreach (var c in member.ClassesTaken
                .OrderByDescending(c => c.Semester.DateStart)
                .ThenBy(c => c.Class.CourseShorthand))
            {
                model.Add(new ClassTranscriptModel
                {
                    Member = member,
                    ClassTaken = c,
                    Grades = GetGrades()
                });
            }

            ViewBag.UserName = userName;
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Transcript(IList<ClassTranscriptModel> model)
        {
            if (!User.IsInRole("Administrator") && !User.IsInRole("Academics") &&
                !model.Any() && User.Identity.GetUserName() != model.First().Member.UserName)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            foreach(var c in model)
            {
                var c1 = c;
                var classTaken = await _db.ClassesTaken
                    .SingleAsync(t => 
                        t.ClassId == c1.ClassTaken.ClassId &&
                        t.SemesterId == c1.ClassTaken.SemesterId &&
                        t.UserId == c1.ClassTaken.UserId);

                classTaken.MidtermGrade = c.ClassTaken.MidtermGrade;
                classTaken.FinalGrade = c.ClassTaken.FinalGrade;
                classTaken.Dropped = c.ClassTaken.Dropped;
                classTaken.IsSummerClass = c.ClassTaken.IsSummerClass;
                _db.Entry(classTaken).State = EntityState.Modified;
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Transcript", new
            {
                userName = model.First().Member.UserName, 
                message = ClassEnrollmentMessageId.UpdateTranscriptSuccess
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadFile(ClassDetailsModel model)
        {
            if (model.FileInfoModel.File == null || model.FileInfoModel.File.ContentLength <= 0) 
                return RedirectToAction("Details", new
                {
                    id = model.Class.ClassId,
                    message = ClassFileMessageId.UploadInvalidFailure
                });
            if (model.FileInfoModel.File.ContentType != "application/pdf") 
                return RedirectToAction("Details", new
                {
                    id = model.Class.ClassId,
                    message = ClassFileMessageId.UploadInvalidFileTypeFailure
                });

            var awsAccessKey = WebConfigurationManager.AppSettings["AWSAccessKey"];
            var awsSecretKey = WebConfigurationManager.AppSettings["AWSSecretKey"];
            var awsBucket = WebConfigurationManager.AppSettings["AWSFileBucket"];

            try
            {
                IAmazonS3 client;
                var key = string.Format(model.Class.CourseShorthand + "/{0}", model.FileInfoModel.File.FileName);
                using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(awsAccessKey, awsSecretKey))
                {
                    var request = new PutObjectRequest()
                    {
                        BucketName = awsBucket,
                        CannedACL = S3CannedACL.Private,
                        Key = key,
                        InputStream = model.FileInfoModel.File.InputStream
                    };

                    client.PutObject(request);
                }

                var file = new ClassFile
                {
                    ClassId = model.Class.ClassId,
                    UserId = User.Identity.GetUserId<int>(),
                    AwsCode = key, 
                    UploadedOn = DateTime.UtcNow,
                };
                _db.ClassFiles.Add(file);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("Details", new
            {
                id = model.Class.ClassId,
                message = ClassFileMessageId.UploadFileSuccess
            });
        }

        public async Task<ActionResult> DownloadFile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            var file = await _db.ClassFiles.FindAsync(id);
            if (file == null)
            {
                return HttpNotFound();
            }

            try
            {
                var awsAccessKey = WebConfigurationManager.AppSettings["AWSAccessKey"];
                var awsSecretKey = WebConfigurationManager.AppSettings["AWSSecretKey"];
                var awsBucket = WebConfigurationManager.AppSettings["AWSFileBucket"];
                IAmazonS3 client;
                using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(awsAccessKey, awsSecretKey))
                {
                    var request = new GetObjectRequest
                    {
                        BucketName = awsBucket, 
                        Key = file.AwsCode
                    };
                    // Make AWS Request for file.
                    var response = client.GetObject(request);

                    using(var memoryStream = new MemoryStream())
                    {
                        // Convert response to a readable format.
                        response.ResponseStream.CopyTo(memoryStream);
                        var contents = memoryStream.ToArray();
                        var fileName = file.AwsCode.Split('/').Last();

                        // Update file access time in database.
                        file.LastAccessedOn = DateTime.UtcNow;
                        file.DownloadCount++;
                        _db.Entry(file).State = EntityState.Modified;
                        await _db.SaveChangesAsync();

                        // Return file to user's browser.
                        return File(contents, "application/pdf", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Details", new
                {
                    id = file.Class.ClassId,
                    message = ClassFileMessageId.DownloadFileAwsFailure
                });
            }
        }

        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteFile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            var file = await _db.ClassFiles.FindAsync(id);
            if (file == null)
            {
                return HttpNotFound();
            }

            return View(file);
        }

        [HttpPost, ActionName("DeleteFile"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Academics")]
        public async Task<ActionResult> DeleteFileConfirmed(int id, int classId)
        {
            var file = await _db.ClassFiles.SingleAsync(f => f.ClassFileId == id);
            if (file == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            try
            {
                var awsAccessKey = WebConfigurationManager.AppSettings["AWSAccessKey"];
                var awsSecretKey = WebConfigurationManager.AppSettings["AWSSecretKey"];
                var awsBucket = WebConfigurationManager.AppSettings["AWSFileBucket"];
                IAmazonS3 client;
                using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(awsAccessKey, awsSecretKey))
                {
                    var request = new DeleteObjectRequest
                    {
                        BucketName = awsBucket,
                        Key = file.AwsCode
                    };
                    client.DeleteObject(request);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Details", new
                {
                    id = classId,
                    message = ClassFileMessageId.DeleteFileAwsFailure
                });
            }

            foreach (var v in file.ClassFileVotes.ToList())
            {
                _db.Entry(v).State = EntityState.Deleted;
            }

            _db.Entry(file).State = EntityState.Deleted;
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new
            {
                id = classId,
                message = ClassFileMessageId.DeleteFileSuccess
            });
        }
        
        public static dynamic GetClassResultMessage(ClassMessageId? message)
        {
            return message == ClassMessageId.DeleteClassFailure 
                    ? "This class is currently being taken, or has been taken, " +
                      "by one or more people, therefore it can't be deleted."
                : message == ClassMessageId.DeleteClassSuccess ? "Class deleted successfully."
                : message == ClassMessageId.AddClassFailure ? "Class cannot be created because a probable duplicate was found."
                : message == ClassMessageId.AddClassSuccess ? "Class created successfully."
                : message == ClassMessageId.EditClassFailure 
                    ? "Class could not be updated.  Check that the class number isn't a duplicated of an existing class."
                : message == ClassMessageId.EditClassSuccess ? "Class updated successfully."
                : "";
        }

        public static dynamic GetEnrollmentResultMessage(ClassEnrollmentMessageId? message)
        {
            return message == ClassEnrollmentMessageId.UpdateTranscriptFailure ? "Failed to update transcript for unknown reason, please contact your administrator."
                : message == ClassEnrollmentMessageId.UpdateTranscriptSuccess ? "Transcript update was successful."
                : message == ClassEnrollmentMessageId.AddClassTakenDuplicateFailure ? "You can't add the same class twice in one semester!"
                : message == ClassEnrollmentMessageId.AddClassTakenUnknownFailure ? "Failed to add a class for some unknown reason, contact your administrator."
                : message == ClassEnrollmentMessageId.AddClassTakenSuccess ? "Class enrollment was successful!"
                : message == ClassEnrollmentMessageId.DeleteClassTakenFailure ? "Could not find the class to remove for this member.  Please try again or contact your administrator if the problem persists."
                : message == ClassEnrollmentMessageId.DeleteClassTakenSuccess ? "Class successfully removed."
                : message == ClassEnrollmentMessageId.EditClassTakenFailure ? "Failed to update class because "
                : message == ClassEnrollmentMessageId.EditClassTakenSuccess ? "Enrollment information for class was updated successfully."
                : "";
        }

        public static dynamic GetFileResultMessage(ClassFileMessageId? message)
        {
            return message == ClassFileMessageId.UploadInvalidFailure ? "Could not upload file because there was nothing selected to upload."
                : message == ClassFileMessageId.UploadInvalidFileTypeFailure ? "Could not upload file because it is not a properly formatted PDF."
                : message == ClassFileMessageId.UploadFileSuccess ? "File was uploaded successfully!"
                : message == ClassFileMessageId.DownloadFileAwsFailure ? "Could not download file because of a server error.  If the problem persists, please contact your administrator."
                : message == ClassFileMessageId.DeleteFileAwsFailure ? "Could not delete file because of a server error.  If the problem persists, please contact your administrator."
                : message == ClassFileMessageId.DeleteFileSuccess ? "File was deleted successfully!"
                : "";
        }

        public enum ClassMessageId
        {
            DeleteClassFailure,
            DeleteClassSuccess,
            AddClassFailure,
            AddClassSuccess,
            EditClassFailure,
            EditClassSuccess
        }

        public enum ClassFileMessageId
        {
            UploadInvalidFailure,
            UploadInvalidFileTypeFailure,
            UploadFileSuccess,
            DownloadFileAwsFailure,
            DeleteFileAwsFailure,
            DeleteFileSuccess,
        }

        public enum ClassEnrollmentMessageId
        {
            UpdateTranscriptFailure,
            UpdateTranscriptSuccess,
            AddClassTakenDuplicateFailure,
            AddClassTakenUnknownFailure,
            AddClassTakenSuccess,
            DeleteClassTakenSuccess,
            DeleteClassTakenFailure,
            EditClassTakenSuccess,
            EditClassTakenFailure
        }
    }
}
