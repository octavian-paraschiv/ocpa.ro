using Microsoft.AspNetCore.Hosting;
using NETCore.MailKit.Core;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Email
{
    public interface IEmailHelper
    {
        Task SendEmail(string[] recipients, string subject, string message);
    }

    public class EmailHelper : BaseHelper, IEmailHelper
    {
        private readonly IEmailService _emailService;

        public EmailHelper(IWebHostEnvironment hostingEnvironment, ILogger logger, IEmailService emailService)
            : base(hostingEnvironment, logger)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public Task SendEmail(string[] recipients, string subject, string message)
            => _emailService.SendAsync(
                mailTo: string.Join("; ", recipients).Trim(),
                subject: subject,
                message: message);
    }
}
