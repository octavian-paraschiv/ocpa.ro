using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ocpa.ro.api.Extensions
{
    public static class ConfigurationExtensions
    {
        public static void ResolveConfiguration<T>(this IConfiguration configuration, IServiceCollection services, string sectionName,
            out T loadedConfiguration) where T : class, new()
        {
            loadedConfiguration = new T();
            var section = configuration.GetSection(sectionName);
            services.Configure<T>(section);
            section.Bind(loadedConfiguration);
        }

        public static void ResolveConfiguration<T>(this IConfiguration configuration, IServiceCollection services, string sectionName) where T : class
        {
            services.Configure<T>(configuration.GetSection(sectionName));
        }
    }
}
