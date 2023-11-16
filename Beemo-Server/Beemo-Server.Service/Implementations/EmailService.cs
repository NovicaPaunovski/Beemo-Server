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
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

            _smtpClient = new SmtpClient(_configuration["EmailServer:Client"]);
            _smtpClient.Port = Convert.ToInt32(_configuration["EmailServer:Port"]);
            _smtpClient.Credentials = new NetworkCredential(_configuration["EmailServer:Credentials"], _configuration["EmailServer:AccessKey"]);
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
            mailMessage.From = new MailAddress(_configuration["EmailServer:Credentials"]);

            return mailMessage;
        }
        #endregion
    }
}
