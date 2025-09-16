using ocpa.ro.common.Extensions;
using System.Collections.Generic;
using System.Net;

namespace ocpa.ro.domain.Models.Content
{
    public enum ContentUnitType
    {
        None = 0,
        Folder,
        MarkdownIndexFolder,
        File
    }


    public class ContentUnit
    {

        public ContentUnitType Type { get; set; }

        public string Name { get; set; }

        private string _path;
        public string Path
        {
            get => _path?.NormalizePath();
            set => _path = value?.NormalizePath();
        }

        public long Size { get; set; } = 0;

        public List<ContentUnit> Children { get; set; }

        public bool Selected { get; set; }
    }

    public class UpdatedContentUnit : ContentUnit
    {
        public HttpStatusCode StatusCode { get; set; }
    }
}
