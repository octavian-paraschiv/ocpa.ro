using ocpa.ro.domain.Abstractions.Database;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Entities.Application;
using ocpa.ro.domain.Extensions;
using ocpa.ro.domain.Models.Configuration;
using Serilog;
using System;
using System.Linq;
using System.Text.Json;

namespace ocpa.ro.application.Services;

public class SystemSettingService : BaseService, ISystemSettingsService
{
    private readonly IApplicationDbContext _dbContext;

    int ISystemSettingsService.ActiveMeteoDbi
    {
        get => ReadSetting<int>(nameof(ISystemSettingsService.ActiveMeteoDbi));
        set => SaveSetting(nameof(ISystemSettingsService.ActiveMeteoDbi), value);
    }

    AuthConfig ISystemSettingsService.AuthenticationSettings
    {
        get => ReadSetting<AuthConfig>(nameof(ISystemSettingsService.AuthenticationSettings), new());
        set => SaveSetting(nameof(ISystemSettingsService.AuthenticationSettings), value);
    }

    CacheConfig ISystemSettingsService.CacheSettings
    {
        get => ReadSetting<CacheConfig>(nameof(ISystemSettingsService.CacheSettings), new());
        set => SaveSetting(nameof(ISystemSettingsService.CacheSettings), value);
    }

    EmailConfig ISystemSettingsService.EmailSettings
    {
        get => ReadSetting<EmailConfig>(nameof(ISystemSettingsService.EmailSettings), new());
        set => SaveSetting(nameof(ISystemSettingsService.EmailSettings), value);
    }

    GeoLocationConfig ISystemSettingsService.GeoLocationSettings
    {
        get => ReadSetting<GeoLocationConfig>(nameof(ISystemSettingsService.GeoLocationSettings), new());
        set => SaveSetting(nameof(ISystemSettingsService.GeoLocationSettings), value);
    }

    void ISystemSettingsService.SeedSettings()
    {
        typeof(ISystemSettingsService).GetProperties().ToList().ForEach(prop =>
        {
            var propVal = prop.GetValue(this);
            prop.SetValue(this, propVal);
        });
    }


    public SystemSettingService(IHostingEnvironmentService hostingEnvironment,
        IApplicationDbContext dbContext,
        ILogger logger)
        : base(hostingEnvironment, logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public T ReadSetting<T>(string key, T defaultValue = default)
    {
        var sVal = _dbContext.SystemSettings
            .FirstOrDefault(ss => ss.Name == key)?
            .Value;

        return CastAs(sVal, defaultValue);
    }

    public int SaveSetting<T>(string key, T val)
    {
        var ss = _dbContext.SystemSettings
            .FirstOrDefault(ss => ss.Name == key);

        if (val is null)
        {
            if (ss is not null)
                return _dbContext.Delete(ss);

            return 0;
        }

        bool newSetting = ss == null;

        var t = typeof(T);

        var sVal = t.IsValueType ?
            val.ToString() : JsonSerializer.Serialize(val);

        ss ??= new SystemSetting { Name = key };

        ss.Value = sVal;
        ss.Type = t.FullName;

        if (newSetting)
            return _dbContext.Insert(ss);

        return _dbContext.Update(ss);
    }

    public T CastAs<T>(string content, T defaultValue)
    {
        if (content?.Length > 0)
        {
            Type t = typeof(T);

            try
            {
                if (t == typeof(string))
                    return (T)(object)content;

                if (t.IsValueType)
                {
                    if (t.IsSubclassOf(typeof(Enum)))
                        return ExtensionMethods.GetEnumValue<T>(content);

                    if (t == typeof(TimeSpan))
                    {
                        if (TimeSpan.TryParse(content, out TimeSpan ts))
                            return (T)(object)ts;

                        return default;
                    }

                    if (t == typeof(DateTime))
                    {
                        if (DateTime.TryParse(content, out DateTime ts))
                            return (T)(object)ts;

                        return default;
                    }

                    // Likely numeric or other integral value type
                    return (T)Convert.ChangeType(content, typeof(T));
                }
            }
            catch (FormatException ex)
            {
                if (t == typeof(bool))
                    return (T)Convert.ChangeType((content ?? "0") != "0", typeof(T));
                else
                    LogException(ex);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            if (!t.IsValueType)
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(content);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
        }

        return defaultValue;
    }

}
