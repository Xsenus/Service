using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Extensions.HostExtensions;
using Service.Model;
using Service.Services;
using Service.Services.TaskQueue;
using Service.Workers;
using System.Threading.Tasks;

namespace Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(confBuilder =>
                {
                    confBuilder.AddJsonFile("config.json");
                    confBuilder.AddCommandLine(args);
                })
                .ConfigureLogging((configLogging) =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                })
                .ConfigureServices((services) =>
                {
                    services.AddHostedService<TasksShedulerService>();
                    services.AddHostedService<WorkerService>();

                    services.AddSingleton<Settings>();
                    services.AddSingleton<TaskProcessor>();
                    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
                });

            await builder.RunService();
        }
    }
}
