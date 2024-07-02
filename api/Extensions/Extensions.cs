﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;

namespace ocpa.ro.api.Extensions
{
    public static class Extensions
    {
        public static void ResolveConfiguration<T>(this IConfiguration configuration, IServiceCollection services, string sectionName,
               out T loadedConfiguration) where T : class, new()
        {
            loadedConfiguration = new T();
            var section = configuration.GetSection(sectionName);
            services.Configure<T>(section);
            section.Bind(loadedConfiguration);
        }

        public static string NormalizePath(this string path) => path.Replace("\\", "/");

        private static readonly ConcurrentDictionary<HttpStatusCode, bool> IsSuccessStatusCode = new ConcurrentDictionary<HttpStatusCode, bool>();

        public static bool IsSuccess(this HttpStatusCode statusCode) =>
            IsSuccessStatusCode.GetOrAdd(statusCode, c => new HttpResponseMessage(c).IsSuccessStatusCode);

        public static string ContentPath(this IWebHostEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
                return null;

            string rootPath = Path.GetDirectoryName(hostingEnvironment.ContentRootPath);
            return Path.Combine(rootPath, "Content");
        }
    }
}
