using Models;

namespace ReportingPortalServer.Services
{
    public class JobSchedulerService(IServiceProvider serviceProvider, ILogger<JobSchedulerService> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<JobSchedulerService> _logger = logger;
        private readonly List<(IScheduledJob job, DateTime nextRun)> _jobs = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            List<IScheduledJob> jobs = scope.ServiceProvider.GetServices<IScheduledJob>().ToList();

            foreach (IScheduledJob job in jobs)
            {
                _jobs.Add((job, DateTime.UtcNow));
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var (job, nextRun) in _jobs.ToList())
                {
                    if (DateTime.UtcNow >= nextRun)
                    {
                        try
                        {
                            _logger.LogInformation("Executing job: {Job}", job.JobName);
                            await job.ExecuteAsync(stoppingToken).ConfigureAwait(false);
                            _jobs.Remove((job, nextRun));
                            _jobs.Add((job, DateTime.UtcNow.Add(job.Interval)));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while executing job {Job}", job.JobName);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
