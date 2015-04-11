namespace DeltaSigmaPhiWebsite.App_Start
{
    using System;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using FluentScheduler;
    using Microsoft.AspNet.Identity;

    public class DspTaskRegistry : Registry
    {
        public DspTaskRegistry()
        {
            //Schedule(async () =>
            //{
            //    var currentTime = ConvertUtcToCst(DateTime.UtcNow);
            //    //if (currentTime.Hour != 5 || currentTime.Minute > 0) return;

            //    try
            //    {
            //        var startOfTodayCst = ConvertUtcToCst(DateTime.UtcNow).Date;
            //        var startOfTodayUtc = ConvertCstToUtc(startOfTodayCst);
            //        var db = new DspDbContext();
            //        var sobers = await db.SoberSchedule
            //            .Where(s => startOfTodayUtc == s.DateOfShift.Date)
            //            .ToListAsync();
            //        if (!sobers.Any()) return;

            //        var message = new IdentityMessage
            //        {
            //            Subject = "Sober Schedule - " + startOfTodayCst.ToShortDateString(),
            //            Body = "<div>",
            //            Destination = "tjm6f4@mst.edu"
            //        };

            //        message.Body += "<h4> Sober Drivers/Officers - " + startOfTodayCst.ToShortDateString() + "</h4>";
            //        message.Body += "<div>";
            //        foreach (var s in sobers)
            //        {
            //            message.Body += @"<span style=""margin-right: 5px"">" + s.Type + "</span>";
            //            message.Body += @"<span style=""margin-right: 5px"">" +
            //                            (s.UserId == null ?
            //                                @"<a href=""https://deltasig-de.org/sphinx/sobers/signup/" + s.SignupId + @""">Sign up</a>" :
            //                                s.Member.FirstName + " " + s.Member.LastName) +
            //                            "</span>";
            //        }
            //        message.Body += "</div>";

            //        var emailService = new EmailService();
            //        await emailService.SendAsync(message);
            //    }
            //    catch (SmtpException e)
            //    {

            //    }
            //}).ToRunEvery(1).Minutes();
        }

        protected virtual DateTime ConvertUtcToCst(DateTime utc)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, cstZone);
        }
        protected virtual DateTime ConvertCstToUtc(DateTime cst)
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(cst, cstZone);
        }

    }
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            var mailMessage = new MailMessage
            {
                From = new MailAddress("sphinxbot@deltasig-de.org", "Sphinx Bot"),
                Subject = "[Sphinx] " + message.Subject,
                Body = "<html><body>" + message.Body + "</body></html>"
            };
            mailMessage.To.Add(message.Destination);
            mailMessage.IsBodyHtml = true;

            var smtpClient = new SmtpClient("mail.deltasig-de.org")
            {
                Port = 26,
                Credentials = new NetworkCredential("sphinxbot@deltasig-de.org", "1q2w#E$R")
            };

            try
            {
                smtpClient.Send(mailMessage);
                return Task.FromResult(1);
            }
            catch (SmtpException e)
            {

            }

            return Task.FromResult(0);
        }
        public Task SendTemplatedAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            var mailMessage = new MailMessage
            {
                From = new MailAddress("sphinxbot@deltasig-de.org", "Sphinx Bot"),
                Subject = "[Sphinx] " + message.Subject,
                Body = message.Body
            };
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
            mailMessage.To.Add(message.Destination);
            mailMessage.IsBodyHtml = true;

            var smtpClient = new SmtpClient("mail.deltasig-de.org")
            {
                Port = 26,
                Credentials = new NetworkCredential("sphinxbot@deltasig-de.org", "1q2w#E$R")
            };

            try
            {
                smtpClient.Send(mailMessage);
                return Task.FromResult(1);
            }
            catch (SmtpException e)
            {

            }

            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            return Task.FromResult(0);
        }
    }
}