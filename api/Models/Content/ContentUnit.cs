using ocpa.ro.api.Extensions;
using System.Collections.Generic;
using System.Net;

namespace ocpa.ro.api.Models.Content
{
    public enum ContentUnitType
    {
        None = 0,
        Folder,
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
    }

    public class UpdatedContentUnit : ContentUnit
    {
        public HttpStatusCode StatusCode { get; set; }
    }
}
