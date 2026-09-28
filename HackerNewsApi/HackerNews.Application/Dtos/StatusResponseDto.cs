namespace HackerNews.Application.Dtos
{
    /// <summary>
    /// Dto for api status
    /// </summary>
    public sealed class StatusResponse
    {
        /// <summary>
        /// API status.
        /// </summary>
        public required string Status { get; init; }

        /// <summary>
        /// API version
        /// </summary>
        public required string Version { get; init; } = string.Empty;
        /// <summary>
        /// API environment
        /// </summary>
        public required string Environment { get; init; } = string.Empty;
        /// <summary>
        /// Current Timestamp in UTC
        /// </summary>
        public required DateTime UtcTimestamp { get; init; } = DateTime.MinValue;
        /// <summary>
        /// API Uptime
        /// </summary>
        public required TimeSpan Uptime { get; init; } = TimeSpan.Zero;
    }
}
