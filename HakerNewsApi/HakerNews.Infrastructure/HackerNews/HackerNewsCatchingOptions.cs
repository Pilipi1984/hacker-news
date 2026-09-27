namespace HackerNews.Infrastructure.HackerNews
{
    public sealed class HackerNewsCatchingOptions
    {
        public const string SectionName = "HackerNews:Caching";

        /// <summary>
        /// Seconds that the Id list is cached
        /// </summary>
        public int IdListCacheSeconds { get; set; }

        /// <summary>
        /// Seconds that each item is cached
        /// </summary>
        public int ItemCacheSeconds { get; set; }
        
        /// <summary>
        /// Maximum number of concurrent upstream requests
        /// </summary>
        public int MaxConcurrentUpstreamRequests { get; set; }
}
