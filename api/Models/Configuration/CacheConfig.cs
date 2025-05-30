namespace ocpa.ro.api.Models.Configuration
{
    public class CacheConfig
    {
        public static readonly string SectionName = "Cache";
        public int CachePeriod { get; set; } = 30;
    }
}
