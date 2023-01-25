using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ocpa.ro.api.Controllers
{
    public class ApiControllerBase : ControllerBase
    {
        protected readonly IWebHostEnvironment _hostingEnvironment;
        protected readonly IAuthHelper _authHelper;
        protected readonly ITokenUtility _tokenUtility;

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

        public ApiControllerBase(IWebHostEnvironment hostingEnvironment, IAuthHelper authHelper, ITokenUtility tokenUtility)
        {
            _hostingEnvironment = hostingEnvironment;
            _authHelper = authHelper;
            _tokenUtility = tokenUtility;
        }
    }
}
