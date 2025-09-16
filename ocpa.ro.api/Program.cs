using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Middlewares;
using ocpa.ro.api.Policies;
using ocpa.ro.common;
using ocpa.ro.common.Extensions;
using ocpa.ro.domain.Abstractions;
using ocpa.ro.domain.Models.Configuration;
using ocpa.ro.Persistence;
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

builder.Services.AddDependencies();

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


builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
    options.UseMySQL(StringUtility.DecodeStrings(databaseConfig.ConnectionString).First()),
    contextLifetime: ServiceLifetime.Transient);



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