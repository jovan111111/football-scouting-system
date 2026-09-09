using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ScoutBoard.Api.Services;

public class EmailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@scoutboard.local";
    public string FromName { get; set; } = "ScoutBoard";
    public bool UseSsl { get; set; } = true;
    public bool DevelopmentMode { get; set; } = true;
}

public interface IEmailService
{
    Task SendVerificationCodeAsync(string email, string firstName, string code);
}

public class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailOptions _options =
        configuration.GetSection("Email").Get<EmailOptions>() ?? new EmailOptions();

    public async Task SendVerificationCodeAsync(string email, string firstName, string code)
    {
        if (_options.DevelopmentMode)
        {
            logger.LogInformation(
                "Razvojni OTP za {Email}: {Code}. Kod važi 10 minuta.",
                email,
                code);
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Host) ||
            string.IsNullOrWhiteSpace(_options.Username) ||
            string.IsNullOrWhiteSpace(_options.Password))
        {
            throw new InvalidOperationException("SMTP podešavanja nisu kompletna.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "ScoutBoard potvrda e-mail adrese";
        message.Body = new TextPart("plain")
        {
            Text = $"""
                    Zdravo {firstName},

                    tvoj ScoutBoard kod za potvrdu e-mail adrese je: {code}

                    Kod važi 10 minuta. Ako nisi otvorio nalog, zanemari ovu poruku.
                    """
        };

        using var client = new SmtpClient();
        var socketOptions = _options.UseSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(_options.Host, _options.Port, socketOptions);
        await client.AuthenticateAsync(_options.Username, _options.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
