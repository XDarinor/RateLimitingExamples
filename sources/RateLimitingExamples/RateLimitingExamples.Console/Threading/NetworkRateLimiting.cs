using Microsoft.Extensions.Logging;
using RateLimitingExamples.Console.Models;
using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace RateLimitingExamples.Console.Threading
{
    internal class NetworkRateLimiting(TimeSpan period, ILogger? logger)
    {
        #region Fields        

        private readonly HttpClient httpClient = new HttpClient();
        private readonly ILogger? logger = logger;

        #endregion

        #region Properties

        public ILogger? Logger => this.logger;

        public TimeSpan Period
        {
            get;
            set;
        } = period;

        #endregion

        #region Methods

        public async Task<GetResult[]> GetAsync(string uri, int numberOfRequests, CancellationToken cancellationToken = default)
        {
            ConcurrentBag<GetResult> getResults = [];
            FixedWindowRateLimiterOptions fixedWindowRateLimiterOptions = new ()
            {
                PermitLimit = 1,
                Window = this.Period
            };

            using RateLimiter rateLimiter = new FixedWindowRateLimiter(fixedWindowRateLimiterOptions);            
            
            await Parallel.ForAsync(0, 
                                    numberOfRequests, 
                                    cancellationToken,
                                    async (i, cancellationToken) =>
                                    {
                                        GetResult? getResult = await this.ExecuteRateLimitedRequest(uri, 
                                                                                                    rateLimiter, 
                                                                                                    cancellationToken: cancellationToken);
                                        if (getResult != null)
                                            getResults.Add(getResult);
                                        else
                                            this.Logger?.LogWarning("Failed to get result. Rate limiter excedeed allowed retries.");
                                    });
            return [.. getResults];
        }

        private async Task<GetResult?> ExecuteRateLimitedRequest(string uri,
                                                                 RateLimiter rateLimiter,
                                                                 int maxRetryCount = 10,
                                                                 CancellationToken cancellationToken = default)
        {
            int retryCount = 0;

            while (retryCount < maxRetryCount)
            {
                using RateLimitLease rateLimitLease = await rateLimiter.AcquireAsync(cancellationToken: cancellationToken);
                if (rateLimitLease.IsAcquired == true)
                {
                    DateTimeOffset acquiredAt = DateTimeOffset.UtcNow;
                    this.Logger?.LogInformation("Execution token acquired at {acquiredAt}.", acquiredAt);

                    HttpResponseMessage result = await this.httpClient.GetAsync(uri, cancellationToken);
                    result.EnsureSuccessStatusCode();

                    string getContent = await result.Content.ReadAsStringAsync(cancellationToken);
                    GetResult getResult = new(getContent, acquiredAt);
                    return getResult;
                }
                else
                {                    
                    if (rateLimitLease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    {
                        this.Logger?.LogWarning("Retrying after {retryAfter}.", retryAfter);
                        await Task.Delay(retryAfter, cancellationToken);
                    }
                    else
                        this.Logger?.LogWarning("Rate limit exceeded. Added to retry count. Current count {count} of {maxCount}",
                                                retryCount,
                                                maxRetryCount);
                    retryCount++;
                }
            }

            return null;
        }

        #endregion
    }
}