using HackerNews.Application.Interfaces;
using HackerNews.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace HackerNews.Infrastructure.HackerNews
{
    /// <summary>
    /// Implements the gateway to the Hacker News API using HTTP calls. 
    /// Each operation makes a separate HTTP request, without caching or concurrency control. 
    /// These responsibilities are handled by the <see cref="CachingHackerNewsGateway"/>, which wraps this class.
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="logger">ILogger</param>
    internal sealed class HackerNewsGateway (HttpClient httpClient, ILogger<HackerNewsGateway> logger) : IHackerNewsGateway
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<HackerNewsGateway> _logger = logger;

        public async Task<int[]> GetBestStoryIdsAsync(CancellationToken ct)
        {
            var ids = await _httpClient.GetFromJsonAsync<int[]>("beststories.json", ct);
            return ids ?? [];
        }

        public async Task<Story?> GetStoryAsync(int id, CancellationToken ct)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<HackerNewsItemResponse>($"item/{id}.json", ct);
                return response is null ? null : MapToDomain(response);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or NotSupportedException)
            {
                _logger.LogWarning(ex, "The story {Id} could not be retrieved from the Hacker News API", id);
                return null;
            }
        }

        private static Story MapToDomain (HackerNewsItemResponse r) => new()
        {
            Id = r.Id,
            Title = r.Title ?? string.Empty,
            Uri = r.Url ?? string.Empty,
            PostedBy = r.By ?? string.Empty,
            Time = DateTimeOffset.FromUnixTimeSeconds(r.Time),
            Score = r.Score,
            CommentCount = r.Descendants,
            IsDeleted = r.Deleted,
            IsDead = r.Dead
        };
    }
}
