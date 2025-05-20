using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ocpa.ro.api.Middlewares
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;


        public GlobalExceptionHandler(RequestDelegate next, ILogger logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleException(context, exception);
            }
        }

        private async Task HandleException(HttpContext context, Exception exception)
        {
            _logger.Error(exception, exception.Message);

            const string contentType = "application/problem+json";
            context.Response.ContentType = contentType;

            context.Response.StatusCode = exception switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };

            await context.Response.WriteAsync(exception.Message);
        }
    }
}
