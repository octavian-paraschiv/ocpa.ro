﻿using Microsoft.AspNetCore.Hosting;
using ocpa.ro.api.Extensions;
using ocpa.ro.api.Models.Content;
using Serilog;
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
        public Task<byte[]> GetContent(string contentPath);

        public ContentUnit ListContent(string contentPath, int? level, string filter);

        public UpdatedContentUnit MoveContent(string contentPath, string newPath);

        public UpdatedContentUnit CreateNewFolder(string contentPath);

        public Task<UpdatedContentUnit> CreateOrUpdateContent(string contentPath, byte[] contentBytes);

        public HttpStatusCode DeleteContent(string contentPath);
    }

    public class ContentHelper : BaseHelper, IContentHelper
    {
        public ContentHelper(IWebHostEnvironment hostingEnvironment, ILogger logger)
            : base(hostingEnvironment, logger)
        {
        }

        public async Task<byte[]> GetContent(string contentPath)
        {
            byte[] data = null;

            try
            {
                var fullContentPath = Path.Combine(_hostingEnvironment.ContentPath(), contentPath);

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
            var unit = ListContent(contentPath, 0, null);

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
            var unit = ListContent(contentPath, 0, null);

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

            var unit = ListContent(contentPath, 0, null);
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

        public UpdatedContentUnit MoveContent(string contentPath, string newPath)
        {
            var unit = ListContent(contentPath, 0, null);

            UpdatedContentUnit ucu = new()
            {
                Children = unit?.Children,
                Path = Path.GetDirectoryName(contentPath),
                Name = Path.GetFileName(contentPath),
                Type = unit?.Type ?? ContentUnitType.None,
                StatusCode = HttpStatusCode.NotFound
            };


            if (unit.Type == ContentUnitType.Folder)
            {
                string path1 = Path.Combine(_hostingEnvironment.ContentPath(), $"{contentPath}");
                string path2 = Path.Combine(_hostingEnvironment.ContentPath(), $"{newPath}");
                Directory.Move(path1, path2);

                ucu.StatusCode = HttpStatusCode.OK;
                ucu.Name = Path.GetFileName(path2);
            }
            else if (unit.Type == ContentUnitType.File)
            {
                string path1 = Path.Combine(_hostingEnvironment.ContentPath(), $"{contentPath}");
                string path2 = Path.Combine(_hostingEnvironment.ContentPath(), $"{newPath}");
                File.Move(path1, path2);

                ucu.StatusCode = HttpStatusCode.OK;
                ucu.Name = Path.GetFileName(path2);
            }

            return ucu;
        }

        public ContentUnit ListContent(string contentPath, int? level, string filter)
        {
            string path = Path.Combine(_hostingEnvironment.ContentPath(), $"{contentPath}");
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
                    Path = Path.GetRelativePath(_hostingEnvironment.ContentPath(), fileInfo.DirectoryName),
                    Size = fileInfo.Length
                };
            }
            else if (FileSystem.Get(path, out DirectoryInfo dirInfo) && dirInfo != null)
            {
                var cu = new ContentUnit
                {
                    Type = ContentUnitType.Folder,
                    Name = dirInfo.Name,
                    Path = Path.GetRelativePath(_hostingEnvironment.ContentPath(), dirInfo.Parent.FullName),
                };

                if (!level.HasValue || level > 0)
                {
                    var dirs = dirInfo.GetDirectories()?
                        .Select(d => ExplorePath(d.FullName, level - 1, filter));
                    //.Where(c => c?.Children?.Count > 0);

                    var files = GetFilesWithComposedFilter(dirInfo, filter)
                        .Select(f => ExplorePath(f.FullName, level - 1, filter))
                        .Where(c => c != null);

                    var list = new List<ContentUnit>();

                    if ((dirs?.Any() ?? false) || (files?.Any() ?? false))
                    {
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
                Path = Path.GetRelativePath(_hostingEnvironment.ContentPath(), Path.GetDirectoryName(path))
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
                string path = Path.Combine(_hostingEnvironment.ContentPath(), $"{contentPath}");
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
                string path = Path.Combine(_hostingEnvironment.ContentPath(), $"{contentPath}");
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
}
