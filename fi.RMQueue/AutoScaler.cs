using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace fi.RMQueue
{
    /// <summary>
    /// AutoScale yapısını açtığınızda kullanılması gereken base sınıfıdır.
    /// </summary>
    public abstract class AutoScaler : BackgroundService
    {
        protected readonly ILogger _logger;
        protected readonly Service _queueService;
        protected readonly TimeSpan autoScaleAwaitTime;

        public AutoScaler(ILogger logger, Service queueService, TimeSpan autoScaleAwaitTime)
        {
            _logger = logger;
            _queueService = queueService;
            this.autoScaleAwaitTime = autoScaleAwaitTime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(autoScaleAwaitTime, stoppingToken);
                _queueService.AutoScale();
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Auto scaler service stopped");

            return base.StopAsync(cancellationToken);
        }
    }
}