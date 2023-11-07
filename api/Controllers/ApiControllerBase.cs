using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ocpa.ro.api.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        protected IActionResult CompressResult(object data)
        {
            string json = JsonConvert.SerializeObject(data);

            using (MemoryStream output = new MemoryStream())
            using (GZipStream gzip = new GZipStream(output, CompressionLevel.Optimal))
            {
                var inData = Encoding.ASCII.GetBytes(json);
                gzip.Write(inData, 0, inData.Length);
                gzip.Close();

                var outData = output.ToArray();

                return Ok(Convert.ToBase64String(outData));
            }
        }
    }
}
