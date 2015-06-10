namespace Dsp.Extensions
{
    using Microsoft.AspNet.Identity;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;

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