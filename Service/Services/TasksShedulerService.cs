using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Model;
using Service.Services.TaskQueue;
using Service.Workers;

namespace Service.Services
{
    public class TasksShedulerService : IHostedService, IDisposable
    {
        public Timer timer;
        private readonly IServiceProvider service;
        private readonly Settings settings;
        private readonly ILogger logger;
        private readonly object syncRoot = new object();

        private readonly Random random = new Random();

        public TasksShedulerService(IServiceProvider service)
        {
            this.service = service;
            this.settings = service.GetRequiredService<Settings>();
            this.logger = service.GetRequiredService<ILogger<TasksShedulerService>>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var interval = settings?.RunInterval ?? 0;

            if (interval == 0)
            {
                logger.LogWarning("CheckInterval is not defined in settings. Set to default: 60 sec.");
                interval = 60;
            }

            timer = new Timer(
                (e) => ProcessTask(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(interval));

            return Task.CompletedTask;
        }

        private void ProcessTask()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                logger.LogInformation($"Process task started");

                for (int i = 0; i < 20; i++)
                {
                    DoWork();
                }

                logger.LogInformation($"Process task finished");
                Monitor.Exit(syncRoot);
            }
            else
            {
                logger.LogInformation($"Processing is currently in progress. Skipped");
            }
        }

        private void DoWork()
        {
            var number = random.Next(20);

            var processor = service.GetRequiredService<TaskProcessor>();
            var queue = service.GetRequiredService<IBackgroundTaskQueue>();

            queue.QueueBackgroundWorkItem(token =>
            {
                return processor.RunAsync(number, token);
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
