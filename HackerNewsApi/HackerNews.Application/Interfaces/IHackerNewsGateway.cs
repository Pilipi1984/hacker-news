using HackerNews.Domain.Entities;

namespace HackerNews.Application.Interfaces
{
    /// <summary>
    /// Port to the Hacker News API
    /// </summary>
    public interface IHackerNewsGateway
    {
        Task<int[]> GetBestStoryIdsAsync(CancellationToken ct);
        Task<Story?> GetStoryAsync(int id, CancellationToken ct);
    }
}
