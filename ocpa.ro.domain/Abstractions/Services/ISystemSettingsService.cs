namespace ocpa.ro.domain.Abstractions.Services;

public interface ISystemSettingsService
{
    T ReadSetting<T>(string key, T defaultValue = default);

    int SaveSetting<T>(string key, T val);
}
