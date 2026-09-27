namespace HackerNews.Infrastructure.HackerNews
{
    public sealed class HackerNewsClientOptions
    {
        public const string SectionName = "HackerNews";

        public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0/";
        public int TimeoutSecond { get; set; } = 10;
    }
}
