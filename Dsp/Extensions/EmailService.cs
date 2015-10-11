namespace Dsp.Extensions
{
    using Microsoft.AspNet.Identity;
    using System.Configuration;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;

    public class EmailService : IIdentityMessageService
    {
        public string EmailServer = ConfigurationManager.AppSettings["EmailServer"];
        public string EmailPort = ConfigurationManager.AppSettings["EmailPort"];
        public string EmailAddress = ConfigurationManager.AppSettings["EmailAddress"];
        public string EmailKey = ConfigurationManager.AppSettings["EmailKey"];

        public Task SendAsync(IdentityMessage message)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(EmailAddress, "Sphinx Bot"),
                Subject = "[Sphinx] " + message.Subject,
                Body = "<html><body>" + message.Body + "</body></html>"
            };
            mailMessage.To.Add(message.Destination);
            mailMessage.IsBodyHtml = true;

            var smtpClient = new SmtpClient(EmailServer, int.Parse(EmailPort))
            {
                Credentials = new NetworkCredential(EmailAddress, EmailKey)
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
            var mailMessage = new MailMessage
            {
                From = new MailAddress(EmailAddress, "Sphinx Bot"),
                Subject = "[Sphinx] " + message.Subject,
                Body = message.Body,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.UTF8
            };
            mailMessage.To.Add(message.Destination);
            mailMessage.IsBodyHtml = true;

            var smtpClient = new SmtpClient(EmailServer, int.Parse(EmailPort))
            {
                Credentials = new NetworkCredential(EmailAddress, EmailKey)
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
}