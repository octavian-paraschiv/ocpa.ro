using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Meteo.Helpers
{
    public static class AppFolders
    {
        public static string StaticImagesFolder { get; private set; }
        public static string DynamicImagesFolder { get; private set; }

        public static string DataFolder { get; private set; }

        public static void Rebuild(HttpRequest request, string viewport)
        {
            ScaleSettings.AppRootPath = request.PhysicalApplicationPath;
            StaticImagesFolder = Path.Combine(request.PhysicalApplicationPath, "Images");
            DynamicImagesFolder = Path.Combine(request.PhysicalApplicationPath, "genImages");
            DataFolder = Path.Combine(request.PhysicalApplicationPath, $"Data\\submatrix_{viewport}");
        }
    }
}