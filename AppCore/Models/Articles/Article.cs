using AppCore.Models.Feeds;
using AppCore.Models.Bookmarks;
using AppCore.Models.Tags;
using System;
using System.Collections.Generic;

namespace AppCore.Models
{
    /// <summary>
    /// Represents an article from a feed
    /// </summary>
    public class Article : BaseEntity
    {
        /// <summary>
        /// Title of the article
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// URL of the article
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Author of the article
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Summary or snippet of the article
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Full content of the article (if fetched)
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Publication date of the article
        /// </summary>
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// Unique identifier from the feed source
        /// </summary>
        public string UniqueId { get; set; } = string.Empty;

        /// <summary>
        /// Whether the article has been read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Whether full content has been fetched
        /// </summary>
        public bool HasFullContent { get; set; }

        /// <summary>
        /// When the full content was last fetched
        /// </summary>
        public DateTime? FullContentFetchedAt { get; set; }

        /// <summary>
        /// ID of the feed this article belongs to
        /// </summary>
        public int FeedId { get; set; }

        /// <summary>
        /// Feed this article belongs to
        /// </summary>
        public virtual Feed Feed { get; set; } = null!;

        /// <summary>
        /// Bookmark associated with this article (if any)
        /// </summary>
        public virtual Bookmark? Bookmark { get; set; }

        /// <summary>
        /// Whether the article is bookmarked
        /// </summary>
        public bool IsBookmarked => Bookmark != null;

        /// <summary>
        /// Time spent reading this article (in seconds)
        /// </summary>
        public int ReadTimeSeconds { get; set; }
    }
}
