using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using TaskManager.Models;

namespace TaskManager.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) || string.IsNullOrWhiteSpace(_emailSettings.SenderPassword))
        {
            _logger.LogWarning("Email settings are not properly configured. Cannot send email to {Email}", email);
            return;
        }

        try
        {
            var cleanedPassword = _emailSettings.SenderPassword.Replace(" ", "").Trim();

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, cleanedPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", email, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via SMTP {Server}:{Port}", email, _emailSettings.SmtpServer, _emailSettings.SmtpPort);
            throw;
        }
    }
}
