namespace AppCore.Models.Settings
{
    /// <summary>
    /// Represents global application settings
    /// </summary>
    public class AppSettings : BaseEntity
    {
        /// <summary>
        /// Global refresh interval for feeds in minutes
        /// </summary>
        public int GlobalRefreshIntervalMinutes { get; set; } = 60;

        /// <summary>
        /// Whether to automatically fetch full content for articles
        /// </summary>
        public bool AutoFetchFullContent { get; set; } = true;

        /// <summary>
        /// Number of seconds to wait before marking an article as read automatically
        /// </summary>
        public int AutoMarkAsReadSeconds { get; set; } = 5;

        /// <summary>
        /// Whether to use a simplified reader mode by default
        /// </summary>
        public bool UseReaderModeByDefault { get; set; } = false;

        /// <summary>
        /// Maximum number of articles to keep per feed (0 means unlimited)
        /// </summary>
        public int MaxArticlesPerFeed { get; set; } = 100;

        /// <summary>
        /// Whether to show article previews in the feed view
        /// </summary>
        public bool ShowArticlePreviews { get; set; } = true;

        /// <summary>
        /// Whether to mark articles as read when scrolled past
        /// </summary>
        public bool MarkAsReadOnScroll { get; set; } = true;
    }
}
