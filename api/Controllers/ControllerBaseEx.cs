using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OPMedia.API.Controllers
{
    public class ControllerBaseEx : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ControllerBaseEx(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        protected string WebRootPath
        {
            get
            {
                return _hostingEnvironment.WebRootPath ??
                    Directory.GetParent(_hostingEnvironment.ContentRootPath).FullName;
            }
        }
    }
}
