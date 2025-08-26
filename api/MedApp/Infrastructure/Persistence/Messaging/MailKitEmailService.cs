using MedApp.Application.Appoinments.Messaging;
using MimeKit;

namespace MedApp.Infrastructure.Persistence.Messaging
{
    public class MailKitEmailService(IConfiguration cfg) : IEmailService 
    {
        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml, byte[]? attachment = null, string? attachmentName = null)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(MailboxAddress.Parse(cfg["EmailSettings:SmtpFrom"]));
            emailMessage.To.Add(MailboxAddress.Parse(toEmail));
            emailMessage.Subject = subject;
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = bodyHtml
            };
            if (attachment != null && attachmentName != null)
            {
                bodyBuilder.Attachments.Add(attachmentName, attachment);
            }
            emailMessage.Body = bodyBuilder.ToMessageBody();
            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
            await smtpClient.ConnectAsync(cfg["EmailSettings:SmtpServer"], int.Parse(cfg["EmailSettings:SmtpPort"] ?? "587"), false);
            if (!string.IsNullOrEmpty(cfg["EmailSettings:SmtpUser"]))
                await smtpClient.AuthenticateAsync(cfg["EmailSettings:SmtpUser"], cfg["EmailSettings:SmtpPass"]);
            await smtpClient.SendAsync(emailMessage);
            await smtpClient.DisconnectAsync(true);
        }

    }
}
