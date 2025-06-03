using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace rssReader.Models
{
    /// <summary>
    /// Represents an RSS feed subscription.
    /// </summary>
    public class Feed
    {
        /// <summary>
        /// Unique identifier for the feed.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Title of the feed.
        /// </summary>
        [JsonProperty("title")]
        [Required(ErrorMessage = "Feed title is required")]
        public string Title { get; set; }

        /// <summary>
        /// Description of the feed.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// URL of the feed.
        /// </summary>
        [JsonProperty("url")]
        [Required(ErrorMessage = "Feed URL is required")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Url { get; set; }

        /// <summary>
        /// URL of the website associated with the feed.
        /// </summary>
        [JsonProperty("websiteUrl")]
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Date and time when the feed was added.
        /// </summary>
        [JsonProperty("dateAdded")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        /// <summary>
        /// Date and time when the feed was last updated/checked.
        /// </summary>
        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        /// <summary>
        /// Custom refresh interval in minutes. If null, the global refresh interval is used.
        /// </summary>
        [JsonProperty("refreshIntervalMinutes")]
        public int? RefreshIntervalMinutes { get; set; }

        /// <summary>
        /// The feed format (RSS, Atom, JSON).
        /// </summary>
        [JsonProperty("format")]
        public FeedFormat Format { get; set; } = FeedFormat.RSS;

        /// <summary>
        /// Logo/icon URL for the feed.
        /// </summary>
        [JsonProperty("logoUrl")]
        public string LogoUrl { get; set; }
        
        /// <summary>
        /// Whether this feed is currently enabled.
        /// </summary>
        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Count of unread articles in this feed.
        /// </summary>
        [JsonIgnore]
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// Represents the format of a feed.
    /// </summary>
    public enum FeedFormat
    {
        RSS,
        Atom,
        JSON
    }
}
