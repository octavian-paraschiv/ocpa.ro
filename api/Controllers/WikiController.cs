using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ocpa.ro.api.Helpers.Wiki;
using System.Threading.Tasks;

namespace ocpa.ro.api.Controllers
{

    [Route("[controller]")]
    [ApiController]
    [ProducesErrorResponseType(typeof(void))]
    public class WikiController : ApiControllerBase
    {
        private readonly IWikiHelper _wikiHelper;

        public WikiController(IWebHostEnvironment hostingEnvironment, IWikiHelper wikiHelper) : base(hostingEnvironment)
        {
            _wikiHelper = wikiHelper;
        }

        [HttpGet("{*resourcePath}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [Produces("text/html")]
        public async Task<IActionResult> GetWikiResource([FromRoute] string resourcePath)
        {
            try
            {
                var ext = System.IO.Path.GetExtension(resourcePath);
                if (ext?.Length > 0)
                {
                    var data = await _wikiHelper.ProcessWikiResource(resourcePath).ConfigureAwait(false);
                    if (data?.Length > 0)
                    {
                        var buffer = new System.ReadOnlyMemory<byte>(data);
                        await Response.BodyWriter.WriteAsync(buffer);
                        return Ok();
                    }
                }
            }
            catch
            {
            }

            return NotFound($"Resource not found: {resourcePath}");
        }
    }
}
