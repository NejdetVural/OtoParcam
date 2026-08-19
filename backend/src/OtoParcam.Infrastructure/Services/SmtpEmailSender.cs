using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using OtoParcam.Application.Common;

namespace OtoParcam.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var emailSection = _configuration.GetSection("Email");
        var host = emailSection["Smtp:Host"];
        var fromAddress = emailSection["FromAddress"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromAddress))
        {
            _logger.LogWarning(
                "Email:Smtp:Host / Email:FromAddress not configured — skipping real send to {ToEmail}. " +
                "Set Email:Smtp:User and Email:Smtp:Password via user-secrets (see CLAUDE.md) to enable it.",
                toEmail);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(emailSection["FromName"] ?? "OtoParcam", fromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        var port = int.Parse(emailSection["Smtp:Port"] ?? "587");
        var useStartTls = bool.Parse(emailSection["Smtp:UseStartTls"] ?? "true");
        var user = emailSection["Smtp:User"];
        var password = emailSection["Smtp:Password"];

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, cancellationToken);

            if (!string.IsNullOrWhiteSpace(user))
            {
                await client.AuthenticateAsync(user, password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            // Sending a real email is best-effort: a broken/misconfigured SMTP server must not fail
            // registration or password-reset requests, which are otherwise fully processed by this point.
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
        }
    }
}
