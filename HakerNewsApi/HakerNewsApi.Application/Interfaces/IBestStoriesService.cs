using HackerNews.Application.Dtos;

namespace HackerNews.Application.Interfaces
{
    public interface IBestStoriesService
    {
        /// <summary>
        /// Get the best n stories
        /// </summary>
        /// <param name="n">number of best stories</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>read only list of best stories</returns>
        Task<IReadOnlyList<StoryDto>> GetBestStoriesAsync(int n, CancellationToken ct);
    }
}
