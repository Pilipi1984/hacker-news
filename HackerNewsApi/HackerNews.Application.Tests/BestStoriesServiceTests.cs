using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Xunit;
using HackerNews.Application.Common;
using HackerNews.Application.Services;
using HackerNews.Application.Dtos;
using HackerNews.Application.Interfaces;
using HackerNews.Domain.Entities;

namespace HackerNews.Application.Tests
{
    public class BestStoriesServiceTests
    {
        [Fact]
        public async Task GetBestStoriesAsync_ThrowsWhenNIsNotPositive()
        {
            var gateway = new FakeGateway();
            var options = Options.Create(new BestStoriesOptions { MaxStories = 5 });
            var sut = new BestStoriesService(gateway, options);

            await Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(() => sut.GetBestStoriesAsync(0, CancellationToken.None));
        }

        [Fact]
        public async Task GetBestStoriesAsync_ReturnsEmptyWhenNoIds()
        {
            var gateway = new FakeGateway { BestIds = new int[0] };
            var options = Options.Create(new BestStoriesOptions { MaxStories = 5 });
            var sut = new BestStoriesService(gateway, options);

            var result = await sut.GetBestStoriesAsync(3, CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBestStoriesAsync_FiltersAndOrdersAndLimits()
        {
            // ids 1..5
            var gateway = new FakeGateway
            {
                BestIds = new[] { 1, 2, 3, 4, 5 },
            };

            // stories: some dead, some deleted, one without title
            gateway.Stories[1] = new Story { Id = 1, Score = 10, Title = "A", Uri = "u", PostedBy = "b" };
            gateway.Stories[2] = new Story { Id = 2, Score = 50, Title = "B", Uri = "u", PostedBy = "b" };
            gateway.Stories[3] = new Story { Id = 3, Score = 5, Title = string.Empty, Uri = "u", PostedBy = "b" };
            gateway.Stories[4] = new Story { Id = 4, Score = 20, Title = "D", Uri = "u", PostedBy = "b", IsDeleted = true };
            gateway.Stories[5] = new Story { Id = 5, Score = 30, Title = "E", Uri = "u", PostedBy = "b" };

            var options = Options.Create(new BestStoriesOptions { MaxStories = 3 });
            var sut = new BestStoriesService(gateway, options);

            var result = await sut.GetBestStoriesAsync(10, CancellationToken.None);

            // should filter out id=3 (empty title) and id=4 (deleted)
            // remaining scores: 50(id2),30(id5),10(id1) -> take max 3
            Assert.Equal(3, result.Count);
            Assert.Equal(new[] { "B", "E", "A" }, result.Select(r => r.Title));
        }

        private class FakeGateway : IHackerNewsGateway
        {
            public int[] BestIds { get; set; } = new[] { 1, 2, 3 };
            public System.Collections.Generic.Dictionary<int, Story?> Stories { get; } = new();

            public FakeGateway()
            {
                // prefill with nulls
                for (int i = 0; i < 10; i++) Stories[i] = null;
            }

            public Task<int[]> GetBestStoryIdsAsync(CancellationToken ct)
            {
                return Task.FromResult(BestIds);
            }

            public Task<Story?> GetStoryAsync(int id, CancellationToken ct)
            {
                Stories.TryGetValue(id, out var s);
                return Task.FromResult(s);
            }
        }
    }
}
