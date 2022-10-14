using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
    }
}
