using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Authentication;
using Serilog;
using System;
using System.Globalization;

namespace ocpa.ro.api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected readonly IWebHostEnvironment _hostingEnvironment;
    protected readonly IAuthHelper _authHelper;
    protected readonly ILogger _logger;

    protected ApiControllerBase(IWebHostEnvironment hostingEnvironment, ILogger logger, IAuthHelper authHelper)
    {
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authHelper = authHelper;
    }

    protected string RequestLanguage
    {
        get
        {
            try
            {
                var ci = new CultureInfo(Request.Headers["X-Language"]);
                if (!string.Equals(ci.TwoLetterISOLanguageName, "en", StringComparison.OrdinalIgnoreCase))
                    return ci.TwoLetterISOLanguageName.ToLowerInvariant();
            }
            catch
            {
            }

            return null;
        }
    }

    protected void LogException(Exception ex) => _logger?.Error(ex, ex.Message);
}
