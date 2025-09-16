using ocpa.ro.common.Extensions;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Models.Content;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ocpa.ro.application.Services;

public class ContentService : BaseService, IContentService
{
    public ContentService(IHostingEnvironmentService hostingEnvironment, ILogger logger)
        : base(hostingEnvironment, logger)
    {
    }

    public async Task<byte[]> GetContent(string contentPath)
    {
        byte[] data = null;

        try
        {
            var fullContentPath = Path.Combine(_hostingEnvironment.ContentPath, contentPath);

            if (File.Exists(fullContentPath))
                data = await File.ReadAllBytesAsync(fullContentPath);
        }
        catch (Exception ex)
        {
            LogException(ex);
            data = null;
        }

        return data;
    }

    public UpdatedContentUnit CreateNewFolder(string contentPath)
    {
        var unit = _ListContent(contentPath, 0, null);

        UpdatedContentUnit ucu = new()
        {
            Children = unit?.Children,
            Path = Path.GetDirectoryName(contentPath),
            Name = Path.GetFileName(contentPath),
            Type = unit?.Type ?? ContentUnitType.None,
            StatusCode = HttpStatusCode.NotFound
        };

        var unitType = unit?.Type ?? ContentUnitType.None;

        if (unitType == ContentUnitType.None)
        {
            if (CreateFolder(contentPath))
                ucu.StatusCode = HttpStatusCode.Created;
            else
                ucu.StatusCode = HttpStatusCode.InternalServerError;
        }
        else if (unitType == ContentUnitType.None)
            ucu.StatusCode = HttpStatusCode.OK; // Already exists, report success
        else
            // The entry is a file
            ucu.StatusCode = HttpStatusCode.Conflict;

        return ucu;
    }

    public async Task<UpdatedContentUnit> CreateOrUpdateContent(string contentPath, byte[] contentBytes)
    {
        var unit = _ListContent(contentPath, 0, null);

        UpdatedContentUnit ucu = new()
        {
            Children = unit?.Children,
            Path = Path.GetDirectoryName(contentPath),
            Name = Path.GetFileName(contentPath),
            Type = unit?.Type ?? ContentUnitType.None,
            StatusCode = HttpStatusCode.NotFound
        };

        var unitType = unit?.Type ?? ContentUnitType.None;

        if (unitType == ContentUnitType.None || unitType == ContentUnitType.File)
        {
            if (await WriteContent(contentPath, contentBytes))
            {
                ucu.StatusCode = unitType == ContentUnitType.None ?
                    HttpStatusCode.Created : HttpStatusCode.OK;
            }
            else
                ucu.StatusCode = HttpStatusCode.InternalServerError;
        }
        else
            // The entry is a folder ...
            ucu.StatusCode = HttpStatusCode.Conflict;

        return ucu;
    }


    public HttpStatusCode DeleteContent(string contentPath)
    {
        bool contentOnly = contentPath.EndsWith("/*");
        if (contentOnly)
            contentPath = contentPath.TrimEnd("/*".ToCharArray());

        var unit = _ListContent(contentPath, 0, null);
        var unitType = unit?.Type ?? ContentUnitType.None;

        string path = Path.Combine(_hostingEnvironment.ContentPath, $"{contentPath}");

        if (unitType == ContentUnitType.None)
            return HttpStatusCode.NotFound;

        if (unitType == ContentUnitType.Folder || unitType == ContentUnitType.MarkdownIndexFolder)
        {
            if (contentOnly)
            {
                var children = Directory.GetFileSystemEntries(path);
                foreach (string p in children)
                {
                    if (Directory.Exists(p))
                        Directory.Delete(p, true);
                    else if (File.Exists(p))
                        File.Delete(p);
                }
            }
            else
                Directory.Delete(path, false);
        }
        else if (unitType == ContentUnitType.File)
            File.Delete(path);

        return HttpStatusCode.OK;
    }

    public UpdatedContentUnit MoveContent(string contentPath, string newPath)
    {
        var unit = _ListContent(contentPath, 0, null);

        UpdatedContentUnit ucu = new()
        {
            Children = unit?.Children,
            Path = Path.GetDirectoryName(contentPath),
            Name = Path.GetFileName(contentPath),
            Type = unit?.Type ?? ContentUnitType.None,
            StatusCode = HttpStatusCode.NotFound
        };


        if (unit.Type == ContentUnitType.Folder || unit.Type == ContentUnitType.MarkdownIndexFolder)
        {
            string path1 = Path.Combine(_hostingEnvironment.ContentPath, $"{contentPath}");
            string path2 = Path.Combine(_hostingEnvironment.ContentPath, $"{newPath}");
            Directory.Move(path1, path2);

            ucu.StatusCode = HttpStatusCode.OK;
            ucu.Name = Path.GetFileName(path2);
        }
        else if (unit.Type == ContentUnitType.File)
        {
            string path1 = Path.Combine(_hostingEnvironment.ContentPath, $"{contentPath}");
            string path2 = Path.Combine(_hostingEnvironment.ContentPath, $"{newPath}");
            File.Move(path1, path2);

            ucu.StatusCode = HttpStatusCode.OK;
            ucu.Name = Path.GetFileName(path2);
        }

        return ucu;
    }

    public ContentUnit ListContent(string contentPath, int? level, string filter, bool markdownView)
    {
        if (markdownView)
            filter = "*.md";

        var content = _ListContent(contentPath, level, filter);

        if (markdownView)
            StripMarkdownIndexFiles(content);

        return content;
    }

    private void StripMarkdownIndexFiles(ContentUnit content)
    {
        if (content.Type == ContentUnitType.MarkdownIndexFolder)
            content.Children = null;

        if (content.Children?.Count > 0)
            content.Children.ForEach(child => StripMarkdownIndexFiles(child));
    }

    private ContentUnit _ListContent(string contentPath, int? level, string filter)
    {
        string path = Path.Combine(_hostingEnvironment.ContentPath, $"{contentPath}");
        return ExplorePath(path, level, filter);
    }

    private ContentUnit ExplorePath(string path, int? level, string filter)
    {
        filter ??= "*";

        if (FileSystem.Get(path, out FileInfo fileInfo) && fileInfo != null)
        {
            return new ContentUnit
            {
                Type = ContentUnitType.File,
                Name = fileInfo.Name,
                Path = Path.GetRelativePath(_hostingEnvironment.ContentPath, fileInfo.DirectoryName),
                Size = fileInfo.Length
            };
        }
        else if (FileSystem.Get(path, out DirectoryInfo dirInfo) && dirInfo != null)
        {
            var cu = new ContentUnit
            {
                Type = ContentUnitType.Folder,
                Name = dirInfo.Name,
                Path = Path.GetRelativePath(_hostingEnvironment.ContentPath, dirInfo.Parent.FullName),
            };

            if (!level.HasValue || level > 0)
            {
                var dirs = dirInfo.GetDirectories()?
                    .Select(d => ExplorePath(d.FullName, level - 1, filter));

                var files = GetFilesWithComposedFilter(dirInfo, filter)
                    .Select(f => ExplorePath(f.FullName, level - 1, filter))
                    .Where(c => c != null);

                var list = new List<ContentUnit>();

                if ((dirs?.Any() ?? false) || (files?.Any() ?? false))
                {
                    if (files?.Any() ?? false)
                    {
                        var markdownIndexFiles = files.Where(f => f.Name.StartsWith("index") && f.Name.EndsWith(".md"));
                        if (markdownIndexFiles?.Any() ?? false)
                        {
                            cu.Type = ContentUnitType.MarkdownIndexFolder;
                            files = files.Where(f => !markdownIndexFiles.Contains(f));
                        }
                    }

                    list.AddRange(dirs);
                    list.AddRange(files);
                    cu.Children = list;
                }
            }

            return cu;
        }

        return new ContentUnit
        {
            Type = ContentUnitType.None,
            Path = Path.GetRelativePath(_hostingEnvironment.ContentPath, Path.GetDirectoryName(path))
        };
    }

    private static FileInfo[] GetFilesWithComposedFilter(DirectoryInfo dirInfo, string composedFilter = "*")
    {
        List<FileInfo> results = new List<FileInfo>();

        var filters = composedFilter.Split('|', StringSplitOptions.RemoveEmptyEntries);
        foreach (var filter in filters)
        {
            var files = dirInfo.GetFiles(filter);
            if (files?.Length > 0)
                results.AddRange(files);
        }

        return results.ToArray();
    }

    private async Task<bool> WriteContent(string contentPath, byte[] contentBytes)
    {
        try
        {
            string path = Path.Combine(_hostingEnvironment.ContentPath, $"{contentPath}");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.WriteAllBytesAsync(path, contentBytes);
            return true;
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return false;
    }

    private bool CreateFolder(string contentPath)
    {
        try
        {
            string path = Path.Combine(_hostingEnvironment.ContentPath, $"{contentPath}");
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return false;
    }
}
