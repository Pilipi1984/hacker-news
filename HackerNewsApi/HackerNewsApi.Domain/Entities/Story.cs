namespace HackerNewsApi.Domain.Entities
{
    /// <summary>
    /// Hacker news story
    /// </summary>
    public class Story
    {
        public required int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string PostedBy { get; set; } = string.Empty;
        public long Time { get; set; }
        public int Score { get; set; }
        public int CommentCount { get; set; }
    }
}
