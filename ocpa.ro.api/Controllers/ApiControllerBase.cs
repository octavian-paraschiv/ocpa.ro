using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Globalization;

namespace ocpa.ro.api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected readonly ILogger _logger;

    protected ApiControllerBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
