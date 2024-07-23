using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ocpa.ro.api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
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
