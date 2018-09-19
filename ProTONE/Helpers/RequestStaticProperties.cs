using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ProTONE.Helpers
{
    public static class AppFolders
    {
        public static string CurrentFolder { get; private set; }
        public static string LegacyFolder { get; private set; }

        public static void Rebuild(HttpRequest request)
        {
            CurrentFolder = Path.Combine(request.PhysicalApplicationPath, "current");
            LegacyFolder = Path.Combine(request.PhysicalApplicationPath, "legacy");
        }
    }
}