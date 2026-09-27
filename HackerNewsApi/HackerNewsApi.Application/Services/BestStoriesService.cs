using HackerNews.Application.Common;
using HackerNews.Application.Dtos;
using HackerNews.Application.Interfaces;
using HackerNews.Domain.Entities;

namespace HackerNews.Application.Services
{
    public class BestStoriesService (IHackerNewsGateway gateway, BestStoriesOptions options) : IBestStoriesService
    {
        private readonly IHackerNewsGateway _gateway = gateway;
        private readonly BestStoriesOptions _options = options;

        public async Task<IReadOnlyList<StoryDto>> GetBestStoriesAsync(int n, CancellationToken ct)
        {
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "The number of stories must be greater than zero.");
            }

            var numberOfStories = Math.Min(n, _options.MaxStories);
            var storyIds = await _gateway.GetBestStoryIdsAsync(ct);

            if(storyIds.Length == 0)
            {
                return [];
            }

            var tasks = storyIds.Select(id => _gateway.GetStoryAsync(id, ct));
            var stories = await Task.WhenAll(tasks);

            return [.. stories
                .Where(story => story is not null && story is {IsDead: false, IsDeleted: false } && !string.IsNullOrEmpty(story.Title))
                .OrderByDescending(story => story!.Score)
                .Take(Math.Min(numberOfStories, stories.Length))
                .Select(MapToDto)];
        }

        private static StoryDto MapToDto(Story? story) => new()
        {
            Title = story?.Title ?? string.Empty,
            Uri = story?.Uri ?? string.Empty,
            PostedBy = story?.PostedBy ?? string.Empty,
            Time = story?.Time ?? DateTimeOffset.MinValue,
            Score = story?.Score ?? 0,
            CommentCount = story?.CommentCount ?? 0
        };
    }
}
