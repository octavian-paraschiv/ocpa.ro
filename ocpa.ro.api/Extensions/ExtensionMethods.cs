using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;

namespace ocpa.ro.api.Extensions;

public static class ExtensionMethods
{
    public static void ResolveConfiguration<T>(this IConfiguration configuration, IServiceCollection services, string sectionName,
           out T loadedConfiguration) where T : class, new()
    {
        loadedConfiguration = new T();
        var section = configuration.GetSection(sectionName);
        services.Configure<T>(section);
        section.Bind(loadedConfiguration);
    }


    private static readonly ConcurrentDictionary<HttpStatusCode, bool> IsSuccessStatusCode = new();

    public static bool IsSuccess(this HttpStatusCode statusCode) =>
        IsSuccessStatusCode.GetOrAdd(statusCode, c => new HttpResponseMessage(c).IsSuccessStatusCode);
}
