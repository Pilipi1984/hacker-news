using HackerNews.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HackerNews.Infrastructure.HackerNews
{
    /// <summary>
    /// Refreshes the best stories cache in the background to ensure that the cache is warm and ready for requests.
    /// </summary>
    /// <param name="gateway">HackerNews gateway instance</param>
    /// <param name="options">Caching options</param>
    /// <param name="logger">Logger instance</param>
    internal sealed class BestStoriesCacheWarmer(IHackerNewsGateway gateway, HackerNewsCachingOptions options, ILogger<BestStoriesCacheWarmer> logger): BackgroundService
    {
        private readonly IHackerNewsGateway _gateway = gateway;
        private readonly HackerNewsCachingOptions _options = options;
        private readonly ILogger<BestStoriesCacheWarmer> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var interval = TimeSpan.FromSeconds(Math.Max(5, _options.IdListCacheSeconds * 0.75));

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _gateway.GetBestStoryIdsAsync(cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error on refreshing best stories cache");
                }

                try
                {
                    await Task.Delay(interval, cancellationToken);
                }
                catch (OperationCanceledException opEx)
                {
                    _logger.LogInformation(opEx, "BestStoriesCacheWarmer is stopping due to cancellation.");
                }
            }
        }
    }
}
