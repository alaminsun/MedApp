namespace MedApp.Application.Appoinments.Messaging
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string bodyHtml, byte[]? attachment = null, string? attachmentName = null);
    }
}
