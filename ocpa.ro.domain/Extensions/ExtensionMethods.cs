using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ocpa.ro.domain.Extensions;

public static class ExtensionMethods
{
    public static string NormalizePath(this string path)
        => path.Replace("\\", "/");

    public static string ToBase64(this string str)
        => str?.Length > 0 ? Convert.ToBase64String(Encoding.UTF8.GetBytes(str)) : null;

    public static int Round(this float input)
        => (int)Math.Round(input);

    public static T GetEnumValue<T>(string value, T defaultValue = default)
    {
        return (from ev in (T[])Enum.GetValues(typeof(T))
                where string.Equals(ev.ToString(), value, StringComparison.OrdinalIgnoreCase)
                select ev).FirstOrCustomValue(defaultValue);
    }

    public static T FirstOrCustomValue<T>(this IEnumerable<T> collection, T customValue)
       => collection.EmptyIfNull().DefaultIfEmpty(customValue).First();

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection)
      => collection ?? new List<T>();
}

public static class StringUtility
{
    public static string EncodeStrings(IEnumerable<string> strings)
        => Encode(new string(string.Join(' ', strings?.Select(Encode))?.Trim()?.Reverse()?.ToArray() ?? []));

    public static IEnumerable<string> DecodeStrings(string str)
        => new string(Decode(str)?.Reverse()?.ToArray() ?? [])?.Split(' ')?.Select(Decode);

    private static string Encode(string str)
        => Convert.ToBase64String(Encoding.ASCII.GetBytes(str));

    private static string Decode(string str)
        => Encoding.ASCII.GetString(Convert.FromBase64String(str));

    public static string AnonymizeEmail(string email)
        => Regex.Replace(email, @"(?<=.).(?=.*@)", "*");
}


public static class JsonProcessing
{
    public static JsonObject AsJsonObject<T>(T t) where T : class
    {
        return t == null ? [] : JsonNode.Parse(JsonSerializer.Serialize(t)) as JsonObject;
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

        return t != null;
    }
}