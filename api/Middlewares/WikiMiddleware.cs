using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ocpa.ro.api.Helpers;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ocpa.ro.api.Middlewares
{
    public static class RequestCultureMiddlewareExtensions
    {
        public static IApplicationBuilder UseWiki(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WikiMiddleware>();
        }
    }

    public class WikiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWikiHelper _wikiHelper;

        public WikiMiddleware(RequestDelegate next, IWikiHelper wikiHelper)
        {
            _next = next;
            _wikiHelper = wikiHelper;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context?.Request?.Path.Value?.EndsWith("protone-suite-docs", System.StringComparison.OrdinalIgnoreCase) ?? false)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                string html = _wikiHelper.GetDefaultResponse();

                if (context.Request.Query.TryGetValue("res", out StringValues res))
                {
                    var mdRes = res.FirstOrDefault();
                    if (mdRes?.Length > 0)
                    {
                        var html2 = await _wikiHelper.ProcessMarkdownFile(mdRes).ConfigureAwait(false);
                        if (html2?.Length > 0)
                        {
                            html = html2;
                            context.Response.StatusCode = StatusCodes.Status200OK;
                        }
                    }
                }

                await context.Response.WriteAsync(html).ConfigureAwait(false);
                return;
            }

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }
}
