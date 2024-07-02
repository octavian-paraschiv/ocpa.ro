namespace ocpa.ro.api.Models.Configuration
{
    public class AuthConfig
    {
        public static readonly string SectionName = "Authorization";

        public bool Disabled { get; set; } = false;
    }
}
