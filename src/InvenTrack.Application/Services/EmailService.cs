using System.Threading.Channels;
using InvenTrack.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace InvenTrack.Application.Services;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class EmailService : IEmailService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly Channel<EmailMessage> _emailChannel;
    private readonly CancellationTokenSource _cts;
    private Task? _processingTask;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _emailChannel = Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions { SingleReader = true });
        _cts = new CancellationTokenSource();
        _processingTask = ProcessEmailQueueAsync(_cts.Token);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var host = emailSettings["Host"];
            var port = int.Parse(emailSettings["Port"] ?? "587");
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];
            var fromAddress = emailSettings["FromAddress"] ?? "noreply@inventrack.local";
            var fromName = emailSettings["FromName"] ?? "InvenTrack Enterprise";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("SMTP credentials not configured. Email to {To} not sent.", to);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
            throw;
        }
    }

    public async Task QueueEmailAsync(string to, string subject, string body)
    {
        await _emailChannel.Writer.WriteAsync(new EmailMessage { To = to, Subject = subject, Body = body });
        _logger.LogDebug("Email queued for {To}: {Subject}", to, subject);
    }

    private async Task ProcessEmailQueueAsync(CancellationToken cancellationToken)
    {
        var reader = _emailChannel.Reader;

        while (await reader.WaitToReadAsync(cancellationToken))
        {
            while (reader.TryRead(out var email))
            {
                // Retry logic: 3 attempts with exponential backoff
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        await SendEmailAsync(email.To, email.Subject, email.Body);
                        break;
                    }
                    catch (Exception ex) when (attempt < 3)
                    {
                        _logger.LogWarning(ex, "Email attempt {Attempt}/3 failed for {To}. Retrying...", attempt, email.To);
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt) * 1000), cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Email failed after 3 attempts for {To}: {Subject}", email.To, email.Subject);
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
