using AppCore.Models.Feeds;
using AppCore.Repositories;
using FluentValidation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppCore.Services.Feeds
{
    /// <summary>
    /// Implementation of the Feed service
    /// </summary>
    public class FeedService : ServiceBase<Feed>, IFeedService
    {
        private readonly IRepository<Article> _articleRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="feedRepository">Repository for feed data access</param>
        /// <param name="articleRepository">Repository for article data access</param>
        /// <param name="validator">Validator for feed entities</param>
        public FeedService(
            IRepository<Feed> feedRepository,
            IRepository<Article> articleRepository,
            IValidator<Feed> validator) 
            : base(feedRepository, validator)
        {
            _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
        }

        /// <summary>
        /// Find a feed by its URL
        /// </summary>
        /// <param name="url">The feed URL to search for</param>
        /// <returns>Feed if found, otherwise null</returns>
        public async Task<Feed?> GetByUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be empty", nameof(url));

            var feeds = await _repository.FindAsync(f => f.FeedUrl == url);
            return feeds.FirstOrDefault();
        }

        /// <summary>
        /// Discover and create a feed from a website URL
        /// </summary>
        /// <param name="websiteUrl">The website URL to examine for feeds</param>
        /// <returns>The discovered and created feed</returns>
        public async Task<Feed?> DiscoverAndCreateFeedAsync(string websiteUrl)
        {
            if (string.IsNullOrWhiteSpace(websiteUrl))
                throw new ArgumentException("Website URL cannot be empty", nameof(websiteUrl));

            // TODO: Implement feed discovery logic
            // This would typically involve:
            // 1. Fetching the website HTML
            // 2. Parsing the HTML to find RSS/Atom/JSON feed links
            // 3. Creating and validating a Feed entity
            // 4. Saving the Feed entity

            // For now, just return null as this would require HTTP capabilities
            // that would be implemented in the infrastructure layer
            return null;
        }

        /// <summary>
        /// Refresh a feed to fetch new articles
        /// </summary>
        /// <param name="feedId">The ID of the feed to refresh</param>
        /// <returns>The refreshed feed</returns>
        public async Task<Feed?> RefreshFeedAsync(int feedId)
        {
            if (feedId <= 0)
                throw new ArgumentException("Feed ID must be greater than zero", nameof(feedId));

            var feed = await _repository.GetByIdAsync(feedId);
            if (feed == null)
                return null;

            // TODO: Implement feed refreshing logic
            // This would typically involve:
            // 1. Fetching the feed content
            // 2. Parsing the feed content
            // 3. Creating new articles or updating existing ones
            // 4. Updating the feed's LastFetchedAt property

            feed.LastFetchedAt = DateTime.UtcNow;
            return await _repository.UpdateAsync(feed);
        }

        /// <summary>
        /// Refresh all feeds
        /// </summary>
        /// <returns>Number of feeds refreshed</returns>
        public async Task<int> RefreshAllFeedsAsync()
        {
            var feeds = await _repository.GetAllAsync();
            int count = 0;

            foreach (var feed in feeds.Where(f => f.IsActive))
            {
                try
                {
                    await RefreshFeedAsync(feed.Id);
                    count++;
                }
                catch (Exception)
                {
                    // Log the exception but continue with other feeds
                    // TODO: Add proper logging
                }
            }

            return count;
        }

        /// <summary>
        /// Update the refresh interval for a feed
        /// </summary>
        /// <param name="feedId">The feed ID</param>
        /// <param name="refreshIntervalMinutes">The new refresh interval in minutes</param>
        /// <returns>The updated feed</returns>
        public async Task<Feed?> UpdateRefreshIntervalAsync(int feedId, int? refreshIntervalMinutes)
        {
            if (feedId <= 0)
                throw new ArgumentException("Feed ID must be greater than zero", nameof(feedId));

            if (refreshIntervalMinutes.HasValue && refreshIntervalMinutes.Value <= 0)
                throw new ArgumentException("Refresh interval must be greater than zero", nameof(refreshIntervalMinutes));

            var feed = await _repository.GetByIdAsync(feedId);
            if (feed == null)
                return null;

            feed.RefreshIntervalMinutes = refreshIntervalMinutes;
            feed.UpdatedAt = DateTime.UtcNow;

            return await _repository.UpdateAsync(feed);
        }

        /// <summary>
        /// Mark all articles in a feed as read
        /// </summary>
        /// <param name="feedId">The feed ID</param>
        /// <returns>Number of articles marked as read</returns>
        public async Task<int> MarkAllArticlesAsReadAsync(int feedId)
        {
            if (feedId <= 0)
                throw new ArgumentException("Feed ID must be greater than zero", nameof(feedId));

            var feed = await _repository.GetByIdAsync(feedId);
            if (feed == null)
                throw new KeyNotFoundException($"Feed with ID {feedId} not found");

            var unreadArticles = await _articleRepository.FindAsync(a => a.FeedId == feedId && !a.IsRead);
            int count = 0;

            foreach (var article in unreadArticles)
            {
                article.IsRead = true;
                article.UpdatedAt = DateTime.UtcNow;
                await _articleRepository.UpdateAsync(article);
                count++;
            }

            // Update feed's unread count
            feed.UnreadCount = 0;
            await _repository.UpdateAsync(feed);

            return count;
        }
    }
}
