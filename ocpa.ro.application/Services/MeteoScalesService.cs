using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Models.Meteo;
using Serilog;
using System;
using System.IO;
using System.Text.Json;

namespace ocpa.ro.application.Services;



public class MeteoScalesService : BaseService, IMeteoScalesService
{
    private MeteoScales _meteoScales = new();
    private readonly string _filePath;

    public InstabilityScale Instability => _meteoScales.Instability;
    public PrecipScale Precip => _meteoScales.Precip;
    public FogScale Fog => _meteoScales.Fog;
    public WindScale Wind => _meteoScales.Wind;
    public TemperatureScale Temperature => _meteoScales.Temperature;
    public PrecipBoundariesScale Boundaries => _meteoScales.Boundaries;


    public MeteoScalesService(IHostingEnvironmentService hostingEnvironment, ILogger logger) :
        base(hostingEnvironment, logger)
    {
        _filePath = Path.Combine(hostingEnvironment.ContentPath, "Meteo/ScaleSettings.json");
        FileInfo fi = new FileInfo(_filePath);

        ReadFile();

        var fsw = new FileSystemWatcher(fi.DirectoryName, $"*{fi.Extension}"); // fi.Extension includes the leading dot character (.)
        fsw.Changed += OnFileChanged;
        fsw.Created += OnFileChanged;
        fsw.Deleted += OnFileChanged;
        fsw.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Created:
            case WatcherChangeTypes.Changed:
                {
                    if (string.Equals(e.FullPath, _filePath, StringComparison.OrdinalIgnoreCase))
                        ReadFile();
                }
                break;

            case WatcherChangeTypes.Deleted:
                ReadFile();
                break;
        }
    }

    private void ReadFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _meteoScales = JsonSerializer.Deserialize<MeteoScales>(json);
                return;
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        // init defaults
        _meteoScales = new();
    }
}
