using ocpa.ro.domain.Abstractions.Gateways;
using ocpa.ro.domain.Abstractions.Services;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ocpa.ro.application.Services;

public class EmailService : BaseService, IEmailService
{
    private readonly IEmailGateway _emailGateway;

    public EmailService(IHostingEnvironmentService hostingEnvironment, ILogger logger, IEmailGateway emailGateway)
        : base(hostingEnvironment, logger)
    {
        _emailGateway = emailGateway ?? throw new ArgumentNullException(nameof(emailGateway));
    }

    public async Task SendOneTimePassword(string recipient, string mfa, string language)
    {
        try
        {
            bool isRomanian = string.Equals(language, "ro", StringComparison.OrdinalIgnoreCase);
            await _emailGateway.SendEmail(
                    recipients: [recipient],

                    subject: isRomanian ?
                        "Conectarea la contul tau OCPA.RO" :
                        "Connect to your OCPA.RO account",

                    message: isRomanian ?
                        $"Pentru conectare la contul tau OCPA.RO, foloseste codul: <b>{mfa}</b>" :
                        $"To connect to your OCPA.RO account, use this code: <b>{mfa}</b>");
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }
}
