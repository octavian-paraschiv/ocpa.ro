using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using ocpa.ro.api.Policies;
using ocpa.ro.domain.Constants;
using System;
using System.Linq;

namespace ocpa.ro.api.Swagger;

public static class SwaggerConfiguration
{
    public const string AccessManagement = "AccessManagement";
    public const string ContentManagement = "ContentManagement";
    public const string CoreServices = "CoreServices";
    public const string Utilities = "Utilities";
    public const string Experimental = "Experimental";

    public static readonly string[] ApiGroups =
    [
        AccessManagement,
        ContentManagement,
        CoreServices,
        Utilities,
        Experimental,
    ];

    public static void AddOpenApiDesc(this IServiceCollection appBuilder)
    {
        ArgumentNullException.ThrowIfNull(appBuilder);

        appBuilder.AddEndpointsApiExplorer();

        appBuilder.AddSwaggerGen(option =>
        {
            option.EnableAnnotations();
            option.DocumentFilter<IgnoreWhenNotInDevFilter>();

            ApiGroups.ToList().ForEach(apiGroup =>
            {
                option.SwaggerDoc(apiGroup, new OpenApiInfo
                {
                    Title = apiGroup,
                    Version = AppConstants.ApiVersion
                });
            });
        });
    }

    public static void UseOpenApiDesc(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSwagger();
        app.UseSwaggerUI(swaggerUiOptions =>
        {
            ApiGroups.ToList().ForEach(api =>
                swaggerUiOptions.SwaggerEndpoint($"{api}/swagger.json", api));
        });
    }
}
