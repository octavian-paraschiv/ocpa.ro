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
    public const string Applications = "Applications";
    public const string Content = "Content";
    public const string Geography = "Geography";
    public const string Menus = "Menus";
    public const string Meteo = "Meteo";
    public const string Experimental = "Experimental";
    public const string ProTONE = "ProTONE";
    public const string Users = "Users";
    public const string Utility = "Utility";

    public static readonly string[] ApiGroups =
    [
        Applications,
        Content,
        Geography,
        Menus,
        Meteo,
        Experimental,
        ProTONE,
        Users,
        Utility
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
