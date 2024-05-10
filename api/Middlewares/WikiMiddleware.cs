using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ocpa.ro.api.Helpers;
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
            var reqPath = context?.Request?.Path.Value ?? string.Empty;

            if (reqPath.StartsWith("/wiki/", System.StringComparison.OrdinalIgnoreCase))
            {
                if (!context.Request?.Method?.Equals(HttpMethods.Get, System.StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    return;
                }


                context.Response.StatusCode = StatusCodes.Status404NotFound;
                string rsp = _wikiHelper.DefaultResponse;

                string resourcePath = reqPath.Replace("/wiki/", string.Empty);
                var ext = System.IO.Path.GetExtension(resourcePath);

                if (ext?.Length > 0)
                {
                    switch (ext.ToUpperInvariant())
                    {
                        case ".MD":
                            var html2 = await _wikiHelper.ProcessWikiFile(resourcePath).ConfigureAwait(false);
                            if (html2?.Length > 0)
                            {
                                rsp = html2;
                                context.Response.StatusCode = StatusCodes.Status200OK;
                            }
                            break;

                        default:
                            var redir = reqPath.Replace("/wiki/", "/Content/wiki/");
                            context.Response.Redirect(redir);
                            return;
                    }
                }

                await context.Response.WriteAsync(rsp).ConfigureAwait(false);
                return;
            }

            await _next(context);
        }
    }
}
