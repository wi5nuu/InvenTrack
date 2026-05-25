using InvenTrack.Application.Interfaces;
using InvenTrack.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvenTrack.Infrastructure.BackgroundJobs;

public class ImportBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImportBackgroundService> _logger;

    public ImportBackgroundService(IServiceProvider serviceProvider, ILogger<ImportBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Import background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var importService = scope.ServiceProvider.GetRequiredService<IImportService>();

                var pendingJobs = await unitOfWork.ImportJobs.GetPendingJobsAsync();

                foreach (var job in pendingJobs)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    _logger.LogInformation("Processing import job {JobId}", job.Id);

                    // We need the concrete ImportService to call ProcessImportJobAsync
                    if (importService is ImportService concreteService)
                    {
                        await concreteService.ProcessImportJobAsync(job);
                    }

                    _logger.LogInformation("Import job {JobId} processed: {Success} success, {Failed} failed",
                        job.Id, job.SuccessRows, job.FailedRows);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in import background service");
            }

            // Poll every 30 seconds
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("Import background service stopped");
    }
}
