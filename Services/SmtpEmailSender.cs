using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Alumni_Management_System.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettingsStore _settings;

    public SmtpEmailSender(EmailSettingsStore settings)
    {
        _settings = settings;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Read per send, so a change saved in Email Settings applies to the
        // very next email.
        var options = _settings.GetEffective();
        if (string.IsNullOrWhiteSpace(options.Host) || string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException("Email is not configured: an Admin needs to set the SMTP server and password in Email Settings.");
        }

        var fromAddress = new MailAddress(options.FromAddress, options.FromName);
        var toAddress = new MailAddress(email);

        using var smtp = new SmtpClient
        {
            Host = options.Host,
            Port = options.Port,
            EnableSsl = options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(options.Username, options.Password)
        };

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        await smtp.SendMailAsync(message);
    }
}
