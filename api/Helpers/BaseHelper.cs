using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;

namespace ocpa.ro.api.Helpers
{
    public abstract class BaseHelper
    {
        protected readonly IWebHostEnvironment _hostingEnvironment;
        protected readonly ILogger _logger;

        protected BaseHelper(IWebHostEnvironment hostingEnvironment, ILogger logger)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected void LogException(Exception ex) => _logger?.Error(ex, ex.Message);
    }
}
