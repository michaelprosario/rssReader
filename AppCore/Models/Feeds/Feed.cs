using System;
using System.Collections.Generic;

namespace AppCore.Models.Feeds
{
    /// <summary>
    /// Represents a feed subscription (RSS, Atom, or JSON)
    /// </summary>
    public class Feed : BaseEntity
    {
        /// <summary>
        /// Title of the feed
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// URL of the feed
        /// </summary>
        public string FeedUrl { get; set; } = string.Empty;

        /// <summary>
        /// Original website URL
        /// </summary>
        public string WebsiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Description of the feed
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Image URL for the feed
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Type of feed (RSS, Atom, JSON)
        /// </summary>
        public FeedType FeedType { get; set; }

        /// <summary>
        /// When the feed was last fetched
        /// </summary>
        public DateTime? LastFetchedAt { get; set; }

        /// <summary>
        /// Custom refresh interval in minutes (null means use global setting)
        /// </summary>
        public int? RefreshIntervalMinutes { get; set; }

        /// <summary>
        /// Collection of articles from this feed
        /// </summary>
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

        /// <summary>
        /// Number of unread articles in this feed
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// Error message if there was a problem fetching the feed
        /// </summary>
        public string? LastFetchError { get; set; }

        /// <summary>
        /// Whether the feed is currently active/enabled
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Type of feed
    /// </summary>
    public enum FeedType
    {
        RSS,
        Atom,
        JSON
    }
}
