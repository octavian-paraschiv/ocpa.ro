using ocpa.ro.domain.Models.Configuration;

namespace ocpa.ro.domain.Abstractions.Services;

public interface ISystemSettingsService
{
    //T ReadSetting<T>(string key, T defaultValue = default);

    //int SaveSetting<T>(string key, T val);

    AuthConfig AuthenticationSettings { get; set; }

    CacheConfig CacheSettings { get; set; }

    EmailConfig EmailSettings { get; set; }

    GeoLocationConfig GeoLocationSettings { get; set; }

    void SeedSettings();
}
