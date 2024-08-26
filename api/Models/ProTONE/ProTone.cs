using System;
using System.Diagnostics.CodeAnalysis;

namespace ocpa.ro.api.Models.ProTONE
{
    public enum BuildType
    {
        Legacy,
        Experimental,
        Release,
    }

    public sealed class BuildVersion : IComparable, IEquatable<BuildVersion>
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }

        public BuildVersion(string version)
        {
            var ver = new Version(version);
            this.Major = ver.Major;
            this.Minor = ver.Minor;
            this.Build = ver.Build;
        }

        public override string ToString() => $"{Major}.{Minor}.{Build}";
        public override int GetHashCode() => ToString().GetHashCode();

        public override bool Equals(object obj) => Equals(obj as BuildVersion);

        public bool Equals([AllowNull] BuildVersion other)
        {
            return other is BuildVersion version &&
                  Major == version.Major &&
                  Minor == version.Minor &&
                  Build == version.Build;
        }

        public int CompareTo(object obj)
        {
            return string.CompareOrdinal(ToString(), (obj as BuildVersion)?.ToString());
        }

        public static bool operator ==(BuildVersion left, BuildVersion right)
        {
            return left?.Equals(right) ?? false;
        }
        public static bool operator >(BuildVersion left, BuildVersion right)
        {
            return (left?.CompareTo(right) ?? 0) > 0;
        }
        public static bool operator <(BuildVersion left, BuildVersion right)
        {
            return (left?.CompareTo(right) ?? 0) < 0;
        }
        public static bool operator !=(BuildVersion left, BuildVersion right)
        {
            return !(left == right);
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
