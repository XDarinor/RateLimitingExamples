using Microsoft.Extensions.Logging;

namespace RateLimitingExamples.Console.Services
{
    internal interface IDownloadDataUsingLimiterService
    {
        #region Consts

        public const int DefaultNumberOfRequests = 10;

        #endregion

        #region Properties

        ILogger<DownloadDataUsingLimiterService> Logger { get; }

        #endregion

        #region Methods

        Task StartAsync(int numberOfRequests = DefaultNumberOfRequests, CancellationToken cancellationToken = default);

        #endregion
    }
}