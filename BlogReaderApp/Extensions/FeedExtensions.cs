using AppCore.Models.Feeds;

namespace BlogReaderApp.Extensions
{
    /// <summary>
    /// Extension methods for feed-related functionality
    /// </summary>
    public static class FeedExtensions
    {
        /// <summary>
        /// Automatically detects the feed type based on URL
        /// </summary>
        /// <param name="url">The URL to analyze</param>
        /// <returns>The detected feed type (defaults to RSS if undetermined)</returns>
        public static FeedType DetectFeedType(this string url)
        {
            if (string.IsNullOrEmpty(url))
                return FeedType.RSS;
                
            string urlLower = url.ToLowerInvariant();
            
            // Check for JSON feed indicators
            if (urlLower.Contains("json") || urlLower.EndsWith(".json") || 
                urlLower.Contains("jsonfeed") || urlLower.Contains("/feed.json"))
                return FeedType.JSON;
                
            // Check for Atom feed indicators 
            if (urlLower.Contains("atom") || urlLower.Contains("/atom/") || 
                urlLower.EndsWith(".atom") || urlLower.Contains("atomfeed"))
                return FeedType.Atom;
                
            // Default to RSS (most common)
            return FeedType.RSS;
        }
    }
}
