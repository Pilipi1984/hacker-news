using HackerNews.Application.Interfaces;
using HackerNews.Domain.Entities;
using HackerNews.Infrastructure.HackerNews;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HackerNews.Infrastructure.Tests
{
    public class CachingHackerNewsGatewayTests
    {
        [Fact]
        public async Task GetBestStoryIdsAsync_CachesResults()
        {
            var inner = new FakeGateway();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var options = new HackerNewsCachingOptions { IdListCacheSeconds = 60, ItemCacheSeconds = 60, MaxConcurrentUpstreamRequests = 2 };

            var sut = new CachingHackerNewsGateway(inner, cache, Microsoft.Extensions.Options.Options.Create(options));

            var ct = CancellationToken.None;

            var first = await sut.GetBestStoryIdsAsync(ct);
            var second = await sut.GetBestStoryIdsAsync(ct);

            Assert.Equal(1, inner.GetBestIdsCallCount);
            Assert.Equal(first, second);
        }

        [Fact]
        public async Task GetStoryAsync_CachesResult()
        {
            var inner = new FakeGateway();
            var cache = new MemoryCache(new MemoryCacheOptions());
            var options = new HackerNewsCachingOptions { IdListCacheSeconds = 60, ItemCacheSeconds = 60, MaxConcurrentUpstreamRequests = 2 };

            var sut = new CachingHackerNewsGateway(inner, cache, Microsoft.Extensions.Options.Options.Create(options));

            var ct = CancellationToken.None;

            var first = await sut.GetStoryAsync(1, ct);
            var second = await sut.GetStoryAsync(1, ct);

            Assert.Equal(1, inner.GetStoryCallCount);
            Assert.NotNull(first);
            Assert.Same(first, second);
        }

        private static CachingHackerNewsGateway CreateSut(IHackerNewsGateway inner, HackerNewsCachingOptions? options = null)
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            return new CachingHackerNewsGateway(inner, cache, Microsoft.Extensions.Options.Options.Create(options ?? new HackerNewsCachingOptions()));
        }

        private class FakeGateway : IHackerNewsGateway
        {
            public int GetBestIdsCallCount { get; private set; }
            public int GetStoryCallCount { get; private set; }

            public Task<int[]> GetBestStoryIdsAsync(CancellationToken ct)
            {
                GetBestIdsCallCount++;
                return Task.FromResult(new[] { 1, 2, 3 });
            }

            public Task<Story?> GetStoryAsync(int id, CancellationToken ct)
            {
                GetStoryCallCount++;
                var s = new Story { Id = id, Title = "t", Uri = "u", PostedBy = "b" };
                return Task.FromResult<Story?>(s);
            }
        }
    }
}
