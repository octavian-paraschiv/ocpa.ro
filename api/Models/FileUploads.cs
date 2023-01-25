using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;


namespace ocpa.ro.api.Models
{
    public class UploadDataPart
    {
        public string PartBase64 { get; set; }
        public int PartIndex { get; set; }
        public int TotalParts { get; set; }
    }
}
