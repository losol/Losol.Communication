using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Losol.Communication.Email.Smtp
{
    public class SmtpEmailSender : AbstractEmailSender
    {
        private readonly SmtpConfig _smtpConfig;
        private readonly ILogger _logger;

        public SmtpEmailSender(IOptions<SmtpConfig> smtpConfig, ILogger<SmtpEmailSender> logger)
        {
            _smtpConfig = smtpConfig.Value;
            _logger = logger;
        }

        public override async Task SendEmailAsync(EmailModel emailModel)
        {
            var mimeMessage = new MimeMessage
            {
                Subject = emailModel.Subject
            };

            mimeMessage.From.Add(emailModel.From != null
                ? new MailboxAddress(Encoding.UTF8, emailModel.From.Name, emailModel.From.Email)
                : new MailboxAddress(_smtpConfig.From));

            mimeMessage.To.AddRange(emailModel.Recipients.Select(a => new MailboxAddress(Encoding.UTF8, a.Name, a.Email)));

            if (emailModel.Cc?.Any() == true)
            {
                mimeMessage.Cc.AddRange(emailModel.Cc.Select(a => new MailboxAddress(Encoding.UTF8, a.Name, a.Email)));
            }

            if (emailModel.Bcc?.Any() == true)
            {
                mimeMessage.Bcc.AddRange(emailModel.Bcc.Select(a => new MailboxAddress(Encoding.UTF8, a.Name, a.Email)));
            }

            var bodyBuilder = new BodyBuilder
            {
                TextBody = emailModel.TextBody,
                HtmlBody = emailModel.HtmlBody
            };

            if (emailModel.Attachments != null)
            {
                foreach (var attachment in emailModel.Attachments)
                {
                    var mimeEntity = bodyBuilder.Attachments.Add(
                        attachment.Filename,
                        attachment.Bytes,
                        ContentType.Parse(attachment.ContentType));

                    if (!string.IsNullOrEmpty(attachment.ContentDisposition))
                    {
                        mimeEntity.ContentDisposition = ContentDisposition.Parse(attachment.ContentDisposition);
                    }

                    if (!string.IsNullOrEmpty(attachment.ContentId))
                    {
                        mimeEntity.ContentId = attachment.ContentId;
                    }
                }
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
    }
}
