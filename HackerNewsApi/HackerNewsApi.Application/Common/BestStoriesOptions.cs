namespace HackerNews.Application.Common
{
    public sealed class BestStoriesOptions
    {
        public const string SectionName = "BestStories";

        public int MaxStories { get; set; } = 300;
    }
}
