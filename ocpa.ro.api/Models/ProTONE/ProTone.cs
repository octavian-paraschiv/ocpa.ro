using System;

namespace ocpa.ro.api.Models.ProTONE
{
    public enum BuildType
    {
        Legacy,
        Experimental,
        Release,
    }

    public sealed class BuildVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }

        public BuildVersion(string version)
        {
            var ver = new Version(version);
            Major = ver.Major;
            Minor = ver.Minor;
            Build = ver.Build;
        }

        public override string ToString() => $"{Major}.{Minor}.{Build}";

        internal bool LessThan(BuildVersion transitionVersion)
        {
            var ts1 = ToString();
            var ts2 = transitionVersion?.ToString() ?? string.Empty;
            return string.Compare(ts1, ts2, StringComparison.OrdinalIgnoreCase) < 0;
        }
    }

    public class BuildInfo
    {
        public string Title { get; set; }

        public BuildVersion Version { get; set; }

        public DateTime BuildDate { get; set; }

        public bool IsRelease { get; set; }

        public string Comment { get; set; }

        public string URL { get; set; }
    }
}
