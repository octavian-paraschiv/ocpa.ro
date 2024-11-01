using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Wiki;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace ocpa.ro.api.Controllers
{

    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    public class WikiController : ApiControllerBase
    {
        private readonly IWikiHelper _wikiHelper;

        public WikiController(IWebHostEnvironment hostingEnvironment, IWikiHelper wikiHelper, ILogger logger)
            : base(hostingEnvironment, logger, null)
        {
            _wikiHelper = wikiHelper;
        }

        [HttpGet("{*resourcePath}")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "application/octet-stream")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [SwaggerOperation(OperationId = "GetWikiResource")]
        public async Task<IActionResult> GetWikiResource([FromRoute] string resourcePath)
        {
            try
            {
                string reqUrl = Request.GetDisplayUrl();
                string reqPath = Request.Path;
                string reqRoot = reqUrl.Replace(reqPath, string.Empty);

                var ext = System.IO.Path.GetExtension(resourcePath);
                if (ext?.Length > 0)
                {
                    var data = await _wikiHelper.ProcessWikiResource(resourcePath, reqRoot).ConfigureAwait(false);
                    if (data?.Length > 0)
                        return File(data, "application/octet-stream");
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return NotFound($"Resource not found: {resourcePath}");
        }
    }
}
