using Azure;
using Beemo_Server.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Beemo_Server.Service.Implementations
{
    public class EmailService : IEmailService
    {
        #region Fields
        private SmtpClient _smtpClient;
        #endregion

        #region Constructor
        public EmailService()
        {
            _smtpClient = new SmtpClient(Environment.GetEnvironmentVariable("BeemoEmailClient"));
            _smtpClient.Port = Convert.ToInt32(Environment.GetEnvironmentVariable("BeemoEmailClientPort"));
            _smtpClient.Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("BeemoEmailCredentials"), Environment.GetEnvironmentVariable("BeemoEmailAccessKey"));
            _smtpClient.EnableSsl = true;
        }
        #endregion

        #region Public Methods
        public void SendEmail(string subject, string body, List<string> recipients)
        {
            var mailMessage = GetMailMessage(subject, body);
            recipients.ForEach(mailMessage.To.Add);

            _smtpClient.Send(mailMessage);
        }

        public void SendEmail(string subject, string body, string recipient)
        {
            var mailMessage = GetMailMessage(subject, body);
            mailMessage.To.Add(recipient);

            _smtpClient.Send(mailMessage);
        }
        #endregion

        #region Private Methods
        private MailMessage GetMailMessage(string subject, string body)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.From = new MailAddress(Environment.GetEnvironmentVariable("BeemoEmailCredentials"));

            return mailMessage;
        }
        #endregion
    }
}
