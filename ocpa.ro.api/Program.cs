using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Middlewares;
using ocpa.ro.api.Swagger;
using ocpa.ro.domain.Extensions;
using System;
using System.IO;
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

#region Services

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(url => true)
        .AllowCredentials());
});


builder.Services.InjectDependencies(builder.Configuration);

builder.Services.AddHttpClients();

builder.Services.AddDistributedMemoryCache();


builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApiDesc();

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
app.UseOpenApiDesc();

await app.RunAsync();
#endregion