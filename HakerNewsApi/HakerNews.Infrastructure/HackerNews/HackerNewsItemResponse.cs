namespace HackerNews.Infrastructure.HackerNews
{
    /// <summary>
    /// Item from Hacker News API
    /// </summary>
    public class HackerNewsItemResponse
    {
        public string? By { get; set; }
        public int Descendants { get; set; }
        public required int Id { get; set; }
        public int Score { get; set; }
        public long Time { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }
        public bool Deleted { get; set; }
        public bool Dead { get; set; }
    }
}
