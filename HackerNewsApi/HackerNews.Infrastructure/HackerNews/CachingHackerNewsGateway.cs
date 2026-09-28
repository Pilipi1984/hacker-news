using HackerNews.Application.Interfaces;
using HackerNews.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HackerNews.Infrastructure.HackerNews
{
    internal sealed class CachingHackerNewsGateway : IHackerNewsGateway
    {
        private const string IdsCacheKey = "hn:beststories:ids";
        private const string ItemCacheKeyPrefix = "hn:item:";

        private readonly IHackerNewsGateway _inner;
        private readonly IMemoryCache _cache;
        private readonly HackerNewsCachingOptions _options;
        private readonly SemaphoreSlim _throttle;

        private readonly SemaphoreSlim _idsRefreshLock = new(1, 1);

        public CachingHackerNewsGateway(IHackerNewsGateway inner, IMemoryCache cache, IOptions<HackerNewsCachingOptions> options)
        {
            _inner = inner;
            _cache = cache;
            _options = options.Value;
            _throttle = new SemaphoreSlim(_options.MaxConcurrentUpstreamRequests);
        }

        public async Task<int[]> GetBestStoryIdsAsync(CancellationToken ct)
        {
            if (_cache.TryGetValue(IdsCacheKey, out int[]? cached) && cached is not null)
            {
                return cached;
            }

            await _idsRefreshLock.WaitAsync(ct);
            try
            {
                if (_cache.TryGetValue(IdsCacheKey, out cached) && cached is not null)
                {
                    return cached;
                }

                var ids = await _inner.GetBestStoryIdsAsync(ct);
                _cache.Set(IdsCacheKey, ids, TimeSpan.FromSeconds(_options.IdListCacheSeconds));

                return ids;
            }
            finally
            {
                _idsRefreshLock.Release();
            }
        }

        public async Task<Story?> GetStoryAsync(int id, CancellationToken ct)
        {
            var cacheKey = ItemCacheKeyPrefix + id;
            if (_cache.TryGetValue(cacheKey, out Story? cached))
            {
                return cached;
            }

            await _throttle.WaitAsync(ct);
            try
            {
                if (_cache.TryGetValue(cacheKey, out cached))
                {
                    return cached;
                }

                var story = await _inner.GetStoryAsync(id, ct);
                _cache.Set(cacheKey, story, TimeSpan.FromSeconds(_options.ItemCacheSeconds));

                return story;
            }
            finally
            {
                _throttle.Release();

            }
        }
    }
}
