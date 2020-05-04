using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Losol.Communication.Email.Smtp
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpConfig _smtpConfig;
        private readonly ILogger _logger;

        public SmtpEmailSender(IOptions<SmtpConfig> smtpConfig, ILogger<SmtpEmailSender> logger)
        {
            _smtpConfig = smtpConfig.Value;
            _logger = logger;
        }

        public async Task SendEmailAsAsync(string fromName, string fromEmail, string address, string subject, string message,
            Attachment attachment = null, EmailMessageType messageType = EmailMessageType.Html)
        {
            var mimeMessage = new MimeMessage();

            mimeMessage.To.Add(MailboxAddress.Parse(address));
            mimeMessage.From.Add(new MailboxAddress(Encoding.UTF8, fromName, fromEmail));

            mimeMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder();

            switch (messageType)
            {
                case EmailMessageType.Text:
                    bodyBuilder.TextBody = message;
                    break;

                case EmailMessageType.Html:
                    bodyBuilder.HtmlBody = message;
                    break;
            }

            if (attachment != null)
            {
                bodyBuilder.Attachments.Add(attachment.Filename, new MemoryStream(attachment.Bytes));
            }

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var emailClient = new SmtpClient();

            try
            {
                _logger.LogInformation($"*** START SEND EMAIL BY SMTP - Smtp host: {_smtpConfig.Host} - Port: {_smtpConfig.Port}***");

                await emailClient.ConnectAsync(_smtpConfig.Host, _smtpConfig.Port, SecureSocketOptions.StartTls);
                if (!string.IsNullOrEmpty(_smtpConfig.Username) &&
                    !string.IsNullOrEmpty(_smtpConfig.Password))
                {
                    await emailClient.AuthenticateAsync(_smtpConfig.Username, _smtpConfig.Password);
                }
                await emailClient.SendAsync(mimeMessage);
                await emailClient.DisconnectAsync(true);

                _logger.LogInformation("*** END SEND EMAIL ***");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public Task SendEmailAsync(
            string address,
            string subject,
            string message,
            Attachment attachment = null,
            EmailMessageType messageType = EmailMessageType.Html)
        {
            var from = MailboxAddress.Parse(_smtpConfig.From);
            return SendEmailAsAsync(from.Name, from.Address, address, subject, message, attachment, messageType);
        }
    }
}
