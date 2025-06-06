using AppCore.Models.Feeds;
using AppCore.Models.Bookmarks;
using AppCore.Models.Tags;
using System;
using System.Collections.Generic;

namespace AppCore.Models
{
    /// <summary>
    /// Represents data for the dashboard view (not persisted)
    /// </summary>
    public class DashboardData
    {
        /// <summary>
        /// Total number of feeds
        /// </summary>
        public int TotalFeeds { get; set; }
        
        /// <summary>
        /// Total number of articles
        /// </summary>
        public int TotalArticles { get; set; }
        
        /// <summary>
        /// Total number of unread articles
        /// </summary>
        public int UnreadArticles { get; set; }
        
        /// <summary>
        /// Total number of bookmarks
        /// </summary>
        public int TotalBookmarks { get; set; }
        
        /// <summary>
        /// Recent bookmarks
        /// </summary>
        public List<Bookmark> RecentBookmarks { get; set; } = new List<Bookmark>();
        
        /// <summary>
        /// Most frequently used tags
        /// </summary>
        public List<Tag> TopTags { get; set; } = new List<Tag>();
        
        /// <summary>
        /// Feeds with the most unread articles
        /// </summary>
        public List<Feed> FeedsWithMostUnread { get; set; } = new List<Feed>();
        
        /// <summary>
        /// Recently updated feeds
        /// </summary>
        public List<Feed> RecentlyUpdatedFeeds { get; set; } = new List<Feed>();
        
        /// <summary>
        /// Most recent articles
        /// </summary>
        public List<Article> RecentArticles { get; set; } = new List<Article>();
        
        /// <summary>
        /// When the dashboard data was last refreshed
        /// </summary>
        public DateTime LastRefreshed { get; set; } = DateTime.UtcNow;
    }
}
