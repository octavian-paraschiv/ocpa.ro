using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ocpa.ro.api.BackgroundServices;
using ocpa.ro.api.Services;
using ocpa.ro.application.Services;
using ocpa.ro.application.Services.Access;
using ocpa.ro.application.Services.Meteo;
using ocpa.ro.domain.Abstractions.Access;
using ocpa.ro.domain.Abstractions.Database;
using ocpa.ro.domain.Abstractions.Gateways;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Extensions;
using ocpa.ro.domain.Models.Configuration;
using ocpa.ro.infrastructure.Gateways;
using ocpa.ro.persistence.ApplicationDb;
using ocpa.ro.persistence.MeteoDb;
using Serilog;
using System.Linq;

namespace ocpa.ro.api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection InjectDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        configuration.ResolveConfiguration(services, DatabaseConfig.SectionName, out DatabaseConfig databaseConfig);

        AddSerilog(services, configuration);
        AddDbContexts(services, databaseConfig);
        AddServices(services);
        AddGateways(services);
        AddJwtAuthentication(services);

        return services;
    }

    private static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        services.AddSingleton(Log.Logger);
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services, DatabaseConfig databaseConfig)
    {
        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
            options.UseMySQL(StringUtility.DecodeStrings(databaseConfig.ConnectionString).First()),
            contextLifetime: ServiceLifetime.Transient);

        services.AddDbContext<IMeteoDbContext, MeteoDbContext>(options =>
            options.UseMySQL(StringUtility.DecodeStrings(databaseConfig.MeteoConnectionString).First()),
            contextLifetime: ServiceLifetime.Transient);

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        var authSettings = services
            .BuildServiceProvider()
            .GetRequiredService<ISystemSettingsService>()
            .AuthenticationSettings;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(o =>
        {
            o.RequireHttpsMetadata = false;
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = authSettings.Jwt.Issuer,
                ValidAudience = authSettings.Jwt.Audience,

                IssuerSigningKey = new SymmetricSecurityKey([.. JwtConfig.KeyBytes]),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            };
        });

        services.AddAuthorization();

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
        services.AddScoped<IMeteoDataService2, MeteoDataService2>();

        services.AddScoped<IMeteoScalesService, MeteoScalesService>();
        services.AddScoped<IWeatherTypeService, WeatherTypeService>();

        services.AddScoped<ISystemSettingsService, SystemSettingService>();

        services.AddSingleton<IHostingEnvironmentService, HostingEnvironmentService>();

        services.AddHostedService<DatabaseManagementService>();
    }

    private static void AddGateways(IServiceCollection services)
    {
        services.AddTransient<IEmailGateway, EmailGateway>();
        services.AddTransient<IGeoLocationGateway, GeoLocationGateway>();
    }
}
