using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ocpa.ro.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseIIS()
                        .UseIISIntegration()
                        .UseStartup<Startup>();
                });
    }
}
