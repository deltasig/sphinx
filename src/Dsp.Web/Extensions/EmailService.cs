﻿namespace Dsp.Web.Extensions
{
    using Dsp.Data;
    using Dsp.Data.Entities;
    using Dsp.Services;
    using Dsp.Services.Interfaces;
    using Elmah;
    using Microsoft.AspNet.Identity;
    using RazorEngine;
    using RazorEngine.Templating;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;

    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            var smtpEndpoint = Environment.GetEnvironmentVariable("SPHINX_SMTP_ENDPOINT");
            var smtpUsername = Environment.GetEnvironmentVariable("SPHINX_SMTP_USERNAME");
            var smtpPassword = Environment.GetEnvironmentVariable("SPHINX_SMTP_PASSWORD");
            var fromAddress = new MailAddress(smtpUsername, "Sphinx");
            var toAddress = new MailAddress(message.Destination);


            SmtpClient client = new SmtpClient()
            {
                Host = smtpEndpoint,
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };
            using (var mailMessage = new MailMessage(fromAddress, toAddress)
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true,
            })
            {
                await client.SendMailAsync(mailMessage);
            }
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

            var emailService = new EmailService();
            await emailService.SendAsync(message);

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

        public static async Task<string> SendTestEmail(string destination)
        {
            var message = new IdentityMessage
            {
                Subject = "Sphinx: Test Email",
                Body = "This is a test email.",
                Destination = destination
            };

            try
            {
                var emailService = new EmailService();
                await emailService.SendAsync(message);
            }
            catch (Exception e)
            {
                ErrorSignal.FromCurrentContext().Raise(e);
                return "Error";
            }
            return "OK";
        }
    }
}