using ocpa.ro.domain.Abstractions.Services;
using Serilog;
using System;

namespace ocpa.ro.application.Services
{
    public abstract class BaseService
    {
        protected readonly IHostingEnvironmentService _hostingEnvironment;
        protected readonly ILogger _logger;

        protected BaseService(IHostingEnvironmentService hostingEnvironment, ILogger logger)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected void LogException(Exception ex) => _logger?.Error(ex, ex.Message);
    }
}
