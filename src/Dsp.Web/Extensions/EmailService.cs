namespace Dsp.Web.Extensions
{
    using Amazon.SimpleEmail;
    using Amazon.SimpleEmail.Model;
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Elmah;
    using Microsoft.AspNet.Identity;
    using RazorEngine;
    using RazorEngine.Templating;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity;
    using System.Linq;
    using System.Net.Mail;
    using System.Threading.Tasks;

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            try
            {
                var awsAccessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
                var awsSecretKey = ConfigurationManager.AppSettings["AWSSecretKey"];
                var emailAddress = ConfigurationManager.AppSettings["EmailAddress"];
                var destination = new Destination(new List<string> { message.Destination });
                var subject = new Content(message.Subject);
                var htmlContent = new Content { Charset = "UTF-8", Data = message.Body };
                var body = new Body { Html = htmlContent };
                var emailMessage = new Message(subject, body);
                var request = new SendEmailRequest(emailAddress, destination, emailMessage);

                using (var client = new AmazonSimpleEmailServiceClient(awsAccessKey, awsSecretKey, Amazon.RegionEndpoint.USEast1))
                {
                    client.SendEmail(request);
                }
                return Task.FromResult(1);
            }
            catch (SmtpException e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            return Task.FromResult(0);
        }

        public static string TryToSendSoberSchedule()
        {
            var db = new SphinxDbContext();
            return TryToSendSoberSchedule(new SoberService(db), db, false).Result;
        }

        public static async Task<string> TryToSendSoberSchedule(ISoberService soberService, SphinxDbContext db, bool isInProperRoles)
        {
            var nowUtc = DateTime.UtcNow;
            var nowCst = nowUtc.FromUtcToCst();

            var type = await db.EmailTypes.SingleOrDefaultAsync(e => e.Name == "Sober Schedule");
            if (string.IsNullOrEmpty(type?.Destination)) return "Bad Request";

            var emails = await db.Emails
                .Where(e =>
                    e.EmailTypeId == type.EmailTypeId &&
                    e.Destination == type.Destination)
                .OrderByDescending(e => e.SentOn)
                .ToListAsync();
            var mostRecentEmail = emails.FirstOrDefault();

            // Check if it has been over 24 hours since the last email.
            var noPreviousEmail = mostRecentEmail == null || (nowUtc - mostRecentEmail.SentOn).TotalHours > 24;
            // Check if the current time is between the arbitrary range.
            var isTime = (nowCst.DayOfWeek == DayOfWeek.Friday &&
                          nowCst.Hour >= 16 && nowCst.Hour < 19);
            // If an admin or the sergeant is trying to manually send the email, just allow it.
            var canOverride = isInProperRoles;

            // Don't send the email if conditions aren't right.
            if ((!isTime || !noPreviousEmail) && !canOverride)
            {
                return "Time: " + isTime + ", Email: " + noPreviousEmail;
            }

            // Build Body
            var data = await soberService.GetUpcomingSignupsAsync(nowUtc);

            if (!data.Any())
            {
                return "No sober signups found; no email sent.";
            }
            var body = Engine.Razor.RunCompile("sse", null, data);
            var bytes = System.Text.Encoding.Default.GetBytes(body);
            body = System.Text.Encoding.UTF8.GetString(bytes);

            var message = new IdentityMessage
            {
                Subject = "Sober Schedule: " +
                nowCst.ToShortDateString() + " - " + nowCst.AddDays(7).ToShortDateString(),
                Body = body,
                Destination = type.Destination
            };

            try
            {
                var emailService = new EmailService();
                await emailService.SendAsync(message);
            }
            catch (SmtpException e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return "Internal Server Error";
            }

            var email = new Email
            {
                SentOn = nowUtc,
                EmailTypeId = type.EmailTypeId,
                Destination = type.Destination,
                Body = body
            };

            db.Emails.Add(email);
            await db.SaveChangesAsync();

            return "OK";
        }
    }
}