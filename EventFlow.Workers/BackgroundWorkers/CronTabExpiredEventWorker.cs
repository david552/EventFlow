using EventFlow.Application.Bookings;
using EventFlow.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Workers.BackgroundWorkers
{
    public class CronTabExpiredEventWorker : BackgroundService
    {
        private readonly ILogger<CronTabExpiredEventWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CrontabSchedule _schedule;
        private DateTime _nextRun;

        private string ScheduleExpression => "*/5 * * * * *";

        public CronTabExpiredEventWorker(
            ILogger<CronTabExpiredEventWorker> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _schedule = CrontabSchedule.Parse(ScheduleExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started. First run scheduled at: {Time}", _nextRun);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;

                if (now >= _nextRun)
                {
                    await Process(stoppingToken);

                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                    _logger.LogInformation("Next run scheduled at: {Time}", _nextRun);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task Process(CancellationToken token)
        {
            _logger.LogInformation("Cleanup process started at {Time}", DateTime.UtcNow);

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

                    await eventService.DeactivateEndedEventsAsync(token);
                }

                _logger.LogInformation("Cleanup process finished successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the cleanup process.");
            }
        }
    }
}
