using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RateLimitingExamples.Console.Services;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)            
            .ConfigureServices((context, services) =>
            {
                services.AddLogging((builder) =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                });

                services.AddTransient<IDownloadDataUsingLimiterService, DownloadDataUsingLimiterService>();
            })
            .Build();


        IDownloadDataUsingLimiterService downloadService = host.Services.GetRequiredService<IDownloadDataUsingLimiterService>();
        await downloadService.StartAsync();

        await host.StopAsync();
    }
}