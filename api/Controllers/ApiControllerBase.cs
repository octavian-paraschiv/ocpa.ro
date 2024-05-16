using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Authentication;
using System.IO;

namespace ocpa.ro.api.Controllers
{
    public class ApiControllerBase : ControllerBase
    {
        protected readonly IWebHostEnvironment _hostingEnvironment;
        protected readonly IAuthHelper _authHelper;

        public string ContentPath
        {
            get
            {
                string rootPath = Path.GetDirectoryName(_hostingEnvironment.ContentRootPath);
                return Path.Combine(rootPath, "Content");
            }
        }

        public ApiControllerBase(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public ApiControllerBase(IWebHostEnvironment hostingEnvironment, IAuthHelper authHelper)
        {
            _hostingEnvironment = hostingEnvironment;
            _authHelper = authHelper;
        }
    }
}
