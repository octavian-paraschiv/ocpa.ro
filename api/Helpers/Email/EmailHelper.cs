using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.Configuration;
using Serilog;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Email
{
    public interface IEmailHelper
    {
        Task SendEmail(string[] recipients, string subject, string message);
        Task SendMfaChallenge(string recipient, string mfa, string language);
    }

    public class EmailHelper : BaseHelper, IEmailHelper
    {
        private readonly EmailConfig _config;

        public EmailHelper(IWebHostEnvironment hostingEnvironment, ILogger logger, IOptions<EmailConfig> config)
            : base(hostingEnvironment, logger)
        {
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        }

        public Task SendMfaChallenge(string recipient, string mfa, string language)
        {
            bool isRomanian = string.Equals(language, "ro", StringComparison.OrdinalIgnoreCase);
            return SendEmail(
                    recipients: [recipient],

                    subject: isRomanian ?
                        "Conectarea la contul tau OCPA.RO" :
                        "Connect to your OCPA.RO account",

                    message: isRomanian ?
                        $"Pentru conectare la contul tau OCPA.RO, foloseste codul: {mfa}" :
                        $"To connect to your OCPA.RO account, use this code: {mfa}");
        }

        public async Task SendEmail(string[] recipients, string subject, string message)
        {
            var mm = new MimeMessage();
            mm.From.Add(new MailboxAddress(_config.FromName, _config.FromAddress));

            recipients.ToList().ForEach(r => mm.To.Add(new MailboxAddress(r, r)));

            mm.Subject = subject;
            mm.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                client.Authenticated += (s, a) => _logger.Debug($"Mail sender authenticated succesfully: {a.Message}");
                client.Connected += (s, a) => _logger.Debug($"Mail sender connected succesfully: {Serialize(a)}");
                client.Disconnected += (s, a) => _logger.Debug($"Mail sender disconnected succesfully: {Serialize(a)}");
                client.MessageSent += (s, a) => _logger.Debug($"Mail sender succesfully sent message: {a.Response}");

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
}
