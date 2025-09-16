using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ocpa.ro.api.BackgroundServices;
using ocpa.ro.api.Services;
using ocpa.ro.application.Services;
using ocpa.ro.application.Services.Access;
using ocpa.ro.domain.Abstractions.Access;
using ocpa.ro.domain.Abstractions.Gateways;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.infrastructure.Gateways;
using Serilog;

namespace ocpa.ro.api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        services.AddSingleton(Log.Logger);
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        return services;
    }

    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        AddServices(services);
        AddGateways(services);

        return services;
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddTransient<IMultipartRequestService, MultipartRequestService>();

        services.AddScoped<IAuthorizationHandler, AuthorizationService>();
        services.AddScoped<IAccessTokenService, AccessTokenService>();
        services.AddScoped<IAccessManagementService, AccessManagementService>();
        services.AddScoped<IAccessService, AccessService>();
        services.AddScoped<IOneTimePasswordService, OneTimePasswordService>();
        services.AddScoped<IContentRendererService, ContentRendererService>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IGeographyService, GeographyService>();
        services.AddScoped<IMeteoDataService, MeteoDataService>();
        services.AddScoped<IMeteoScalesService, MeteoScalesService>();
        services.AddScoped<IWeatherTypeService, WeatherTypeService>();

        services.AddSingleton<IHostingEnvironmentService, HostingEnvironmentService>();

        services.AddHostedService<DatabaseCleanupService>();
    }

    private static void AddGateways(IServiceCollection services)
    {
        services.AddTransient<IEmailGateway, EmailGateway>();
        services.AddTransient<IGeoLocationGateway, GeoLocationGateway>();
    }
}
