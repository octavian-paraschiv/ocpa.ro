using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Extensions;
using System;
using System.IO;

namespace ocpa.ro.api.Services;

public class HostingEnvironmentService : IHostingEnvironmentService
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public HostingEnvironmentService(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));

    }

    public string ContentRootPath => _hostingEnvironment.ContentRootPath.GetFullPath(true);

    public string ContentPath
    {
        get
        {
            if (_hostingEnvironment == null)
                return null;

            string rootPath = Path.GetDirectoryName(_hostingEnvironment.ContentRootPath);

            if (_hostingEnvironment.IsDevelopment())
                return Path.Combine(rootPath, "Content").GetFullPath(true);

            return Path.Combine(rootPath, "../Content").GetFullPath(true);
        }
    }
}
