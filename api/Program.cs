using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ocpa.ro.api.Extensions;
using System;
using System.IO;

namespace ocpa.ro.api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var isDev = string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development", StringComparison.OrdinalIgnoreCase);

            string logDir = "Logs";

            if (!isDev)
            {
                var dllDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                logDir = Path.Combine(dllDir, "../../content/Logs").NormalizePath();
            }

            Environment.SetEnvironmentVariable("LOGDIR", logDir);

            Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder
                       .UseIIS()
                       .UseIISIntegration()
                       .UseStartup<Startup>();
               })
               .Build()
               .Run();
        }
    }
}
