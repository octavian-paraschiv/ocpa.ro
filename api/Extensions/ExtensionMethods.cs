using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ocpa.ro.api.Extensions
{
    public static class ExtensionMethods
    {
        public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            services.AddSingleton(Log.Logger);
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            return services;
        }

        public static void ResolveConfiguration<T>(this IConfiguration configuration, IServiceCollection services, string sectionName,
               out T loadedConfiguration) where T : class, new()
        {
            loadedConfiguration = new T();
            var section = configuration.GetSection(sectionName);
            services.Configure<T>(section);
            section.Bind(loadedConfiguration);
        }

        public static string NormalizePath(this string path) => path.Replace("\\", "/");

        private static readonly ConcurrentDictionary<HttpStatusCode, bool> IsSuccessStatusCode = new();

        public static bool IsSuccess(this HttpStatusCode statusCode) =>
            IsSuccessStatusCode.GetOrAdd(statusCode, c => new HttpResponseMessage(c).IsSuccessStatusCode);

        public static string ContentPath(this IWebHostEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
                return null;

            string rootPath = Path.GetDirectoryName(hostingEnvironment.ContentRootPath);

            if (hostingEnvironment.IsDevelopment())
                return Path.Combine(rootPath, "Content");

            return Path.Combine(rootPath, "../Content");
        }

        public static int Round(this float input)
        {
            return (int)Math.Round(input);
        }

        public static T GetValue<T>(this Dictionary<string, float> data, string key, T defaultValue = default)
            where T : IComparable, IConvertible, IFormattable
        {
            Type type = typeof(T);

            T val;
            try
            {
                double raw = data[key];

                if (type != typeof(float) &&
                    type != typeof(double) &&
                    type != typeof(decimal))
                {
                    raw = Math.Round(raw, 0);
                }

                val = (T)Convert.ChangeType(raw, type);
            }
            catch
            {
                val = defaultValue;
            }

            return val;
        }

        public static string ToBase64(this string str)
            => (str?.Length > 0) ? Convert.ToBase64String(Encoding.UTF8.GetBytes(str)) : null;
    }

    public static class FileSystem
    {
        public static bool Get<T>(string path, out T t) where T : FileSystemInfo
        {
            try
            {
                bool isFile = typeof(T) == typeof(FileInfo);
                FileSystemInfo fsi = isFile ? new FileInfo(path) : new DirectoryInfo(path);
                t = fsi.Exists ? fsi as T : null;
            }
            catch
            {
                t = null;
            }

            return (t != null);
        }
    }

    public static class JsonProcessing
    {
        public static JsonObject AsJsonObject<T>(T t) where T : class
        {
            return (t == null) ? [] : JsonNode.Parse(JsonSerializer.Serialize(t)) as JsonObject;
        }

        public static void Merge(this JsonObject target, JsonObject source)
        {
            foreach (var kvp in source)
            {
                if (kvp.Value is JsonArray sourceArray && target[kvp.Key] is JsonArray targetArray)
                {
                    // Merge arrays by union
                    foreach (var item in sourceArray)
                    {
                        if (!targetArray.Contains(item))
                        {
                            targetArray.Add(item);
                        }
                    }
                }
                else if (kvp.Value is JsonObject sourceObject && target[kvp.Key] is JsonObject targetObject)
                {
                    // Recursively merge objects
                    targetObject.Merge(sourceObject);
                }
                else if (kvp.Value is not null)
                {
                    // Overwrite or add the value if it's not null
                    target[kvp.Key] = kvp.Value;
                }
                // If kvp.Value is null, do nothing (ignore null values)
            }
        }
    }

    public static class StringEncoding
    {
        public static string EncodeStrings(IEnumerable<string> strings)
        => Encode(new string(string.Join(' ', strings?.Select(s => Encode(s)))?.Trim()?.Reverse()?.ToArray() ?? []));

        public static IEnumerable<string> DecodeStrings(string str)
        => new string(Decode(str)?.Reverse()?.ToArray() ?? [])?.Split(' ')?.Select(s => Decode(s));

        private static string Encode(string str)
        => Convert.ToBase64String(Encoding.ASCII.GetBytes(str));

        private static string Decode(string str)
        => Encoding.ASCII.GetString(Convert.FromBase64String(str));
    }
}
