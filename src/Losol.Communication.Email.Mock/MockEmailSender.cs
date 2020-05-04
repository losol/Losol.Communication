using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Losol.Communication.Email.Mock
{
    public class MockEmailSender : IEmailSender
    {
        private readonly ILogger<MockEmailSender> _logger;

        public MockEmailSender(ILogger<MockEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string address, string subject, string message, Attachment attachment = null,
            EmailMessageType messageType = EmailMessageType.Html)
        {
            _logger.LogInformation("Sending email with subject {subject} to {address}", subject, address);
            return Task.CompletedTask;
        }

        public Task SendEmailAsAsync(string fromName, string fromEmail, string address, string subject, string message,
            Attachment attachment = null, EmailMessageType messageType = EmailMessageType.Html)
        {
            _logger.LogInformation("Sending email from {{fromEmail}} <{{fromName}}> with subject {{subject}} to {{address}}",
                fromEmail, fromName, subject, address);
            return Task.CompletedTask;
        }
    }
}
