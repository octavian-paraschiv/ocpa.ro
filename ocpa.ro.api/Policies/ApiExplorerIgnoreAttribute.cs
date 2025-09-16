using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;

namespace ocpa.ro.api.Policies
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class IgnoreWhenNotInDevAttribute : Attribute
    {
        public IgnoreWhenNotInDevAttribute()
        {
        }
    }

    public class IgnoreWhenNotInDevFilter : IDocumentFilter
    {
        private readonly bool _isDev;

        public IgnoreWhenNotInDevFilter(IWebHostEnvironment env)
        {
            _isDev = env?.IsDevelopment() ?? false;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (!_isDev)
            {
                foreach (var apiDescription in context.ApiDescriptions)
                {
                    if (apiDescription?.ActionDescriptor is ControllerActionDescriptor cad)
                    {
                        var attr =
                            cad.MethodInfo?.GetCustomAttribute<IgnoreWhenNotInDevAttribute>() ??
                            cad.MethodInfo?.DeclaringType?.GetCustomAttribute<IgnoreWhenNotInDevAttribute>();

                        if (attr != null)
                        {
                            var route = $"/{apiDescription.RelativePath}";
                            swaggerDoc.Paths.Remove(route);
                        }
                    }
                }
            }
        }
    }
}