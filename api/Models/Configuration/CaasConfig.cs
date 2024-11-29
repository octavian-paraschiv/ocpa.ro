namespace ocpa.ro.api.Models.Configuration
{
    public class CaasConfig
    {
        public static readonly string SectionName = "CaaS";
        public string BaseUrl { get; set; } = "";
        public int RefreshPeriod { get; set; } = 30;
    }
}
