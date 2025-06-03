using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace rssReader.Models
{
    /// <summary>
    /// Represents a user-defined tag for organizing articles.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Unique identifier for the tag.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name of the tag.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of the tag (optional).
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Date when the tag was created.
        /// </summary>
        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        /// <summary>
        /// Color associated with the tag (optional, for UI purposes).
        /// </summary>
        [JsonProperty("color")]
        public string Color { get; set; }

        /// <summary>
        /// Number of articles using this tag (calculated, not stored)
        /// </summary>
        [JsonIgnore]
        public int ArticleCount { get; set; }
    }

    /// <summary>
    /// Represents application settings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Global refresh interval in minutes.
        /// </summary>
        [JsonProperty("globalRefreshIntervalMinutes")]
        public int GlobalRefreshIntervalMinutes { get; set; } = 60; // Default: 1 hour

        /// <summary>
        /// Whether to automatically fetch full content for articles.
        /// </summary>
        [JsonProperty("autoFetchFullContent")]
        public bool AutoFetchFullContent { get; set; } = false;

        /// <summary>
        /// Whether to automatically mark articles as read when scrolled past.
        /// </summary>
        [JsonProperty("autoMarkAsRead")]
        public bool AutoMarkAsRead { get; set; } = true;

        /// <summary>
        /// Default view mode for articles.
        /// </summary>
        [JsonProperty("defaultViewMode")]
        public ViewMode DefaultViewMode { get; set; } = ViewMode.Normal;
        
        /// <summary>
        /// Whether to show article previews in feed view.
        /// </summary>
        [JsonProperty("showArticlePreviews")]
        public bool ShowArticlePreviews { get; set; } = true;

        /// <summary>
        /// Date and time of the last refresh operation.
        /// </summary>
        [JsonProperty("lastRefreshDate")]
        public DateTime LastRefreshDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Data storage directory path.
        /// </summary>
        [JsonProperty("dataDirectoryPath")]
        public string DataDirectoryPath { get; set; } = "AppData";
    }

    /// <summary>
    /// View modes for article display.
    /// </summary>
    public enum ViewMode
    {
        Normal,
        Reader
    }
}
