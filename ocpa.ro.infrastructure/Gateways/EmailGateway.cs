using MailKit.Net.Smtp;
using MimeKit;
using ocpa.ro.domain.Abstractions.Gateways;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Extensions;
using ocpa.ro.domain.Models.Configuration;
using Serilog;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ocpa.ro.infrastructure.Gateways;

public class EmailGateway : IEmailGateway
{
    private readonly ILogger _logger;
    private readonly EmailConfig _config;

    public EmailGateway(ILogger logger, ISystemSettingsService settingsService)
    {
        _config = settingsService.EmailSettings;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendEmail(string[] recipients, string subject, string message)
    {
        var mm = new MimeMessage();
        mm.From.Add(new MailboxAddress(_config.FromName, _config.FromAddress));

        recipients.ToList().ForEach(r => mm.To.Add(new MailboxAddress(r, r)));

        mm.Subject = subject;
        mm.Body = new TextPart("html") { Text = message };

        using (var client = new SmtpClient())
        {
            client.Authenticated += (s, a) => _logger.Debug($"Mail client authenticated succesfully: {a.Message}");
            client.Connected += (s, a) => _logger.Debug($"Mail client connected succesfully: {Serialize(a)}");
            client.Disconnected += (s, a) => _logger.Debug($"Mail client disconnected succesfully: {Serialize(a)}");
            client.MessageSent += (s, a) => _logger.Debug($"Mail client succesfully sent message: {a.Response}");

            client.Connect(_config.ServerAddress, _config.ServerPort, _config.ServerPort != 25);

            if (_config?.Credentials?.Length > 0)
            {
                var creds = StringUtility.DecodeStrings(_config.Credentials).ToArray();
                await client.AuthenticateAsync(creds[0], creds[1]);
            }

            await client.SendAsync(mm);
            await client.DisconnectAsync(true);
        }
    }

    private string Serialize(object obj) => JsonSerializer.Serialize(obj);
}
