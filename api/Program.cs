﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ocpa.ro.api;
using ocpa.ro.api.BackgroundServices;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Helpers.Authentication;
using ocpa.ro.api.Helpers.Content;
using ocpa.ro.api.Helpers.Email;
using ocpa.ro.api.Helpers.Generic;
using ocpa.ro.api.Helpers.Geography;
using ocpa.ro.api.Helpers.Medical;
using ocpa.ro.api.Helpers.Meteo;
using ocpa.ro.api.Middlewares;
using ocpa.ro.api.Models.Configuration;
using ocpa.ro.api.Persistence;
using ocpa.ro.api.Policies;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

string logDir = "Logs";

if (!isDevelopment)
{
    var dllDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
    logDir = Path.Combine(dllDir, "../../../Logs").NormalizePath();
}

Environment.SetEnvironmentVariable("LOGDIR", logDir);

#region ConfigurationResolving
builder.Configuration.ResolveConfiguration(builder.Services, AuthConfig.SectionName, out AuthConfig authConfig);
builder.Configuration.ResolveConfiguration(builder.Services, GeoLocationConfig.SectionName, out GeoLocationConfig _);
builder.Configuration.ResolveConfiguration(builder.Services, CacheConfig.SectionName, out CacheConfig _);
builder.Configuration.ResolveConfiguration(builder.Services, EmailConfig.SectionName, out EmailConfig emailConfig);
builder.Configuration.ResolveConfiguration(builder.Services, DatabaseConfig.SectionName, out DatabaseConfig databaseConfig);
#endregion

#region Services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(url => true)
        .AllowCredentials());
});

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = authConfig.Jwt.Issuer,
        ValidAudience = authConfig.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(JwtConfig.KeyBytes.ToArray()),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddAuthorization();

builder.Services.AddHttpClients();

builder.Services.AddScoped<IAuthHelper, AuthHelper>();
builder.Services.AddScoped<IGeographyHelper, GeographyHelper>();
builder.Services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();

builder.Services.AddScoped<IMeteoDataHelper, MeteoDataHelper>();
builder.Services.AddScoped<IMeteoScalesHelper, MeteoScalesHelper>();
builder.Services.AddScoped<IWeatherTypeHelper, WeatherTypeHelper>();
builder.Services.AddScoped<IMedicalDataHelper, MedicalDataHelper>();
builder.Services.AddScoped<IAuthorizationHandler, AuthorizePolicy>();
builder.Services.AddScoped<IContentHelper, ContentHelper>();

builder.Services.AddTransient<IMultipartRequestHelper, MultipartRequestHelper>();
builder.Services.AddTransient<IContentRenderer, ContentRenderer>();
builder.Services.AddTransient<IEmailHelper, EmailHelper>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSerilog(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSwaggerGen(option =>
{
    option.EnableAnnotations();
    option.DocumentFilter<IgnoreWhenNotInDevFilter>();

    option.SwaggerDoc(Constants.ApiVersion,
        new OpenApiInfo
        {
            Title = Constants.AppName,
            Version = Constants.ApiVersion
        });
});


builder.Services.AddDbContext<IDbContext, ApplicationDbContext>(options =>
    options.UseMySQL(StringUtility.DecodeStrings(databaseConfig.ConnectionString).First()),
    contextLifetime: ServiceLifetime.Transient);

builder.Services.AddHostedService<DatabaseCleanupService>();

#endregion

#region App
var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();

if (isDevelopment)
    app.UseDeveloperExceptionPage();

app.UseRouting();

if (isDevelopment)
{
    app.UseCors(x => x
        .AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(url => true)
        .AllowCredentials());
}
else
    app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

await app.RunAsync();
#endregion