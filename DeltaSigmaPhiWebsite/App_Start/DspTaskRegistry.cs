namespace DeltaSigmaPhiWebsite.App_Start
{
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using FluentScheduler;
    using Microsoft.AspNet.Identity;

    public class DspTaskRegistry : Registry
    {
        public DspTaskRegistry()
        {
            Schedule(async () =>
            {
                var message = new IdentityMessage
                {
                    Subject = "Check check",
                    Body = "I'm rick james.",
                    Destination = "tjm6f4@mst.edu"
                };

                try
                {
                    var emailService = new EmailService();
                    await emailService.SendAsync(message);
                }
                catch (SmtpException e)
                {

                }
            }).ToRunNow().AndEvery(3).Days().At(19, 0);
        }
    }
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            var mailMessage = new MailMessage
            {
                From = new MailAddress("***REMOVED***", "Sphinx Bot"),
                Subject = message.Subject,
                Body = message.Body
            };
            mailMessage.To.Add(message.Destination);

            var smtpClient = new SmtpClient("mail.deltasig-de.org")
            {
                Port = 26,
                Credentials = new NetworkCredential("***REMOVED***", "***REMOVED***")
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