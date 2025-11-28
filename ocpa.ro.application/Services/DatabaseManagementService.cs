using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ocpa.ro.domain.Abstractions.Database;
using ocpa.ro.domain.Abstractions.Services;
using ocpa.ro.domain.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ocpa.ro.api.BackgroundServices;

public class DatabaseManagementService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    const int PERIODICITY = 1;

    public DatabaseManagementService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<ISystemSettingsService>().SeedSettings();
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var query = $"DELETE FROM OneTimePassword WHERE Expiration <= '{DateTime.UtcNow.ToString(AppConstants.DateTimeFormat)}'";
                dbContext.ExecuteSqlRaw(query);
            }

            await Task.Delay(TimeSpan.FromMinutes(PERIODICITY), stoppingToken);
        }
    }
}
