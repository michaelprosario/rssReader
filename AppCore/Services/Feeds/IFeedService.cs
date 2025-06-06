using AppCore.Models.Feeds;
using System.Threading.Tasks;

namespace AppCore.Services.Feeds
{
    /// <summary>
    /// Interface for Feed service operations
    /// </summary>
    public interface IFeedService : IService<Feed>
    {
        /// <summary>
        /// Find a feed by its URL
        /// </summary>
        /// <param name="url">The feed URL to search for</param>
        /// <returns>Feed if found, otherwise null</returns>
        Task<Feed?> GetByUrlAsync(string url);

        /// <summary>
        /// Discover and create a feed from a website URL
        /// </summary>
        /// <param name="websiteUrl">The website URL to examine for feeds</param>
        /// <returns>The discovered and created feed</returns>
        Task<Feed?> DiscoverAndCreateFeedAsync(string websiteUrl);

        /// <summary>
        /// Refresh a feed to fetch new articles
        /// </summary>
        /// <param name="feedId">The ID of the feed to refresh</param>
        /// <returns>The refreshed feed</returns>
        Task<Feed?> RefreshFeedAsync(int feedId);

        /// <summary>
        /// Refresh all feeds
        /// </summary>
        /// <returns>Number of feeds refreshed</returns>
        Task<int> RefreshAllFeedsAsync();

        /// <summary>
        /// Update the refresh interval for a feed
        /// </summary>
        /// <param name="feedId">The feed ID</param>
        /// <param name="refreshIntervalMinutes">The new refresh interval in minutes</param>
        /// <returns>The updated feed</returns>
        Task<Feed?> UpdateRefreshIntervalAsync(int feedId, int? refreshIntervalMinutes);

        /// <summary>
        /// Mark all articles in a feed as read
        /// </summary>
        /// <param name="feedId">The feed ID</param>
        /// <returns>Number of articles marked as read</returns>
        Task<int> MarkAllArticlesAsReadAsync(int feedId);
    }
}
