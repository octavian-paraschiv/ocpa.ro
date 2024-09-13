using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Authentication;
using Serilog;
using System;

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

    protected void LogException(Exception ex) => _logger?.Error(ex, ex.Message);
}
