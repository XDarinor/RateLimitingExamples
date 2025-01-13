using Microsoft.Extensions.Logging;
using RateLimitingExamples.Console.Models;
using RateLimitingExamples.Console.Threading;

namespace RateLimitingExamples.Console.Services
{
    internal class DownloadDataUsingLimiterService(ILogger<DownloadDataUsingLimiterService> logger) 
        : IDownloadDataUsingLimiterService
    {
        #region Consts       

        #endregion

        #region Fields

        private readonly ILogger<DownloadDataUsingLimiterService> logger = logger;

        #endregion

        #region Properties

        public ILogger<DownloadDataUsingLimiterService> Logger => this.logger;

        #endregion

        #region Methods

        public async Task StartAsync(int numberOfRequests = IDownloadDataUsingLimiterService.DefaultNumberOfRequests, CancellationToken cancellationToken = default)
        {
            NetworkRateLimiting networkRateLimiting = new(TimeSpan.FromSeconds(3), this.Logger);
            string uri = "https://httpbin.org/get";

            this.Logger?.LogInformation("Starting to download data using rate limiter.");
            GetResult[] getResults = await networkRateLimiting.GetAsync(uri, numberOfRequests, cancellationToken);
            this.Logger?.LogInformation("Downloaded data using rate limiter.");

            foreach(GetResult getResult in getResults)
            {
                this.Logger?.LogInformation("Request executed at: {0}", getResult.AcquiredAt);
            }            

            foreach (GetResult getResult in getResults)
            {                
                this.Logger?.LogInformation("Data retrieved: " + 
                                            Environment.NewLine + 
                                            "{getResult}", 
                                            getResult.ToString());
            }
        }

        #endregion
    }
}
