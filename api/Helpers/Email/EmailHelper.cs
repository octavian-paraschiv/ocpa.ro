using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Email
{
    public interface IEmailHelper
    {
        Task<string> SendEmail(string[] recipients, string subject, string message);
    }

    public class EmailHelper : BaseHelper, IEmailHelper
    {
        private IFluentEmail _fluentEmail;

        public EmailHelper(IWebHostEnvironment hostingEnvironment, ILogger logger, IFluentEmail fluentEmail)
            : base(hostingEnvironment, logger)
        {
            _fluentEmail = fluentEmail ?? throw new ArgumentNullException(nameof(fluentEmail));
        }

        public async Task<string> SendEmail(string[] recipients, string subject, string message)
        {
            var rsp = await _fluentEmail
                    .To(recipients.Select(r => new Address(r)))
                    .Subject(subject)
                    .Body(message)
                    .SendAsync();

            if (!(rsp?.Successful ?? false))
            {
                if (rsp.ErrorMessages?.Count > 0)
                    return string.Join("\n", rsp.ErrorMessages);
                else if (rsp.MessageId?.Length > 0)
                    return rsp.MessageId;
                else return "Unknown error";
            }

            return null;
        }

    }
}
