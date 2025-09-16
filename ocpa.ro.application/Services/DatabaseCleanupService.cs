using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ocpa.ro.domain.Abstractions;

namespace ocpa.ro.api.BackgroundServices;

public class DatabaseCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    const int PERIODICITY = 1;

    public DatabaseCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var query = $"DELETE FROM OneTimePassword WHERE Expiration <= '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}'";
                dbContext.ExecuteSqlRaw(query);
            }

            await Task.Delay(TimeSpan.FromMinutes(PERIODICITY), stoppingToken);
        }
    }
}
