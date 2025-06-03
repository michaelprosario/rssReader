using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace rssReader.Models
{
    /// <summary>
    /// Represents an article from an RSS feed.
    /// </summary>
    public class Article
    {
        /// <summary>
        /// Unique identifier for the article.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The ID of the feed this article belongs to.
        /// </summary>
        [JsonProperty("feedId")]
        public string FeedId { get; set; }

        /// <summary>
        /// Original ID or GUID from the feed.
        /// </summary>
        [JsonProperty("originalId")]
        public string OriginalId { get; set; }

        /// <summary>
        /// Title of the article.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Summary or short description of the article.
        /// </summary>
        [JsonProperty("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// Full content of the article, if available.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// URL to the original article.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Publication date of the article.
        /// </summary>
        [JsonProperty("publishDate")]
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// Date when the article was discovered/added to the reader.
        /// </summary>
        [JsonProperty("discoveryDate")]
        public DateTime DiscoveryDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Authors of the article.
        /// </summary>
        [JsonProperty("authors")]
        public List<string> Authors { get; set; } = new List<string>();

        /// <summary>
        /// Categories or tags associated with the article in the original feed.
        /// </summary>
        [JsonProperty("categories")]
        public List<string> Categories { get; set; } = new List<string>();

        /// <summary>
        /// Whether the article has been read.
        /// </summary>
        [JsonProperty("isRead")]
        public bool IsRead { get; set; }

        /// <summary>
        /// Whether the article has been bookmarked.
        /// </summary>
        [JsonProperty("isBookmarked")]
        public bool IsBookmarked { get; set; }

        /// <summary>
        /// Whether full content has been fetched.
        /// </summary>
        [JsonProperty("isFullContentFetched")]
        public bool IsFullContentFetched { get; set; }

        /// <summary>
        /// Date when the article was read.
        /// </summary>
        [JsonProperty("readDate")]
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// Date when the article was bookmarked.
        /// </summary>
        [JsonProperty("bookmarkDate")]
        public DateTime? BookmarkDate { get; set; }

        /// <summary>
        /// User-assigned tags for this article.
        /// </summary>
        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Optional image URL associated with the article.
        /// </summary>
        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Feed title (for display purposes, not stored)
        /// </summary>
        [JsonIgnore]
        public string FeedTitle { get; set; }
    }
}
