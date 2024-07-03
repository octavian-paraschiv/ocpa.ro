using Microsoft.AspNetCore.Hosting;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Content
{
    public interface IContentHelper
    {
        public ContentUnit ListContent(string contentPath, int? level, string filter);

        public Task<UpdatedContentUnit> CreateOrUpdateContent(string contentPath, byte[] contentBytes);

        public HttpStatusCode DeleteContent(string contentPath);
    }

    public class ContentHelper : IContentHelper
    {
        private readonly IWebHostEnvironment _hostingEnvironment = null;

        public ContentHelper(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        public async Task<UpdatedContentUnit> CreateOrUpdateContent(string contentPath, byte[] contentBytes)
        {
            var unit = ListContent(contentPath, 0);

            UpdatedContentUnit ucu = new UpdatedContentUnit
            {
                Children = unit?.Children,
                Path = contentPath,
                Type = unit?.Type ?? ContentUnitType.None,
                StatusCode = HttpStatusCode.NotFound
            };

            var unitType = unit?.Type ?? ContentUnitType.None;

            if (unitType == ContentUnitType.None || unitType == ContentUnitType.File)
            {
                if (await WriteContent(contentPath, contentBytes))
                {
                    ucu.StatusCode = unit.Type == ContentUnitType.None ?
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

            var unit = ListContent(contentPath, 0);
            var unitType = unit?.Type ?? ContentUnitType.None;

            string path = Path.Combine(_hostingEnvironment.ContentPath(), $"{contentPath}");

            if (unitType == ContentUnitType.None)
                return HttpStatusCode.NotFound;

            if (unitType == ContentUnitType.Folder)
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

        public ContentUnit ListContent(string contentPath, int? level, string filter = null)
        {
            string path = Path.Combine(_hostingEnvironment.ContentPath(), $"{contentPath}");
            return ExplorePath(path, level, filter);
        }

        private ContentUnit ExplorePath(string path, int? level, string filter)
        {
            filter ??= "*";

            if (File.Exists(path))
            {
                return new ContentUnit
                {
                    Type = ContentUnitType.File,
                    Name = Path.GetFileName(path),
                    Path = Path.GetRelativePath(_hostingEnvironment.ContentPath(), Path.GetDirectoryName(path))
                };
            }
            else if (Directory.Exists(path))
            {
                if (!level.HasValue || level > 0)
                {
                    var dirs = Directory.GetDirectories(path)?
                        .Select(p => ExplorePath(p, level - 1, filter))
                        .Where(c => c?.Children?.Count > 0);

                    var files = Directory.GetFiles(path, filter)?
                        .Select(p => ExplorePath(p, level - 1, filter))
                        .Where(c => c != null);

                    var list = new List<ContentUnit>();

                    if ((dirs?.Any() ?? false) || (files?.Any() ?? false))
                    {
                        list.AddRange(dirs);
                        list.AddRange(files);

                        return new ContentUnit
                        {
                            Type = ContentUnitType.Folder,
                            Name = Path.GetFileName(path),
                            Path = Path.GetRelativePath(_hostingEnvironment.ContentPath(), Path.GetDirectoryName(path)),
                            Children = list
                        };
                    }
                }
                else
                {
                    return new ContentUnit
                    {
                        Type = ContentUnitType.Folder,
                        Name = Path.GetFileName(path),
                        Path = Path.GetRelativePath(_hostingEnvironment.ContentPath(), Path.GetDirectoryName(path))
                    };
                }
            }

            return new ContentUnit
            {
                Type = ContentUnitType.None,
                Path = Path.GetRelativePath(_hostingEnvironment.ContentPath(), Path.GetDirectoryName(path))
            };
        }

        private async Task<bool> WriteContent(string contentPath, byte[] contentBytes)
        {
            try
            {
                string path = Path.Combine(_hostingEnvironment.ContentPath(), $"{contentPath}");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                await File.WriteAllBytesAsync(path, contentBytes);
                return true;
            }
            catch
            {
            }

            return false;
        }
    }
}
