using AppCore.Models;
using AppCore.Models.Feeds;
using AppCore.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppCore.Services.Articles
{
    /// <summary>
    /// Implementation of the Article service
    /// </summary>
    public class ArticleService : ServiceBase<Article>, IArticleService
    {
        private readonly IRepository<Feed> _feedRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="articleRepository">Repository for article data access</param>
        /// <param name="feedRepository">Repository for feed data access</param>
        /// <param name="validator">Validator for article entities</param>
        public ArticleService(
            IRepository<Article> articleRepository,
            IRepository<Feed> feedRepository,
            IValidator<Article> validator)
            : base(articleRepository, validator)
        {
            _feedRepository = feedRepository ?? throw new ArgumentNullException(nameof(feedRepository));
        }

        /// <summary>
        /// Get all unread articles
        /// </summary>
        /// <returns>List of unread articles</returns>
        public async Task<IEnumerable<Article>> GetUnreadArticlesAsync()
        {
            return await _repository.FindAsync(a => !a.IsRead);
        }

        /// <summary>
        /// Get all articles for a specific feed
        /// </summary>
        /// <param name="feedId">ID of the feed</param>
        /// <returns>Articles for the specified feed</returns>
        public async Task<IEnumerable<Article>> GetArticlesByFeedAsync(int feedId)
        {
            if (feedId <= 0)
                throw new ArgumentException("Feed ID must be greater than zero", nameof(feedId));

            // Verify the feed exists
            var feedExists = await _feedRepository.ExistsAsync(f => f.Id == feedId);
            if (!feedExists)
                throw new KeyNotFoundException($"Feed with ID {feedId} not found");

            return await _repository.FindAsync(a => a.FeedId == feedId);
        }

        /// <summary>
        /// Mark an article as read
        /// </summary>
        /// <param name="articleId">ID of the article</param>
        /// <returns>The updated article</returns>
        public async Task<Article?> MarkAsReadAsync(int articleId)
        {
            var article = await _repository.GetByIdAsync(articleId);
            if (article == null)
                return null;

            if (!article.IsRead)
            {
                article.IsRead = true;
                article.UpdatedAt = DateTime.UtcNow;
                
                // Update the feed's unread count
                var feed = await _feedRepository.GetByIdAsync(article.FeedId);
                if (feed != null)
                {
                    feed.UnreadCount = Math.Max(0, feed.UnreadCount - 1);
                    await _feedRepository.UpdateAsync(feed);
                }
                
                return await _repository.UpdateAsync(article);
            }
            
            return article; // Already read, no need to update
        }

        /// <summary>
        /// Mark an article as unread
        /// </summary>
        /// <param name="articleId">ID of the article</param>
        /// <returns>The updated article</returns>
        public async Task<Article?> MarkAsUnreadAsync(int articleId)
        {
            var article = await _repository.GetByIdAsync(articleId);
            if (article == null)
                return null;

            if (article.IsRead)
            {
                article.IsRead = false;
                article.UpdatedAt = DateTime.UtcNow;
                
                // Update the feed's unread count
                var feed = await _feedRepository.GetByIdAsync(article.FeedId);
                if (feed != null)
                {
                    feed.UnreadCount++;
                    await _feedRepository.UpdateAsync(feed);
                }
                
                return await _repository.UpdateAsync(article);
            }
            
            return article; // Already unread, no need to update
        }

        /// <summary>
        /// Fetch full content for an article
        /// </summary>
        /// <param name="articleId">ID of the article</param>
        /// <returns>Article with full content</returns>
        public async Task<Article?> FetchFullContentAsync(int articleId)
        {
            var article = await _repository.GetByIdAsync(articleId);
            if (article == null)
                return null;

            // TODO: Implement full content fetching logic
            // This would typically involve:
            // 1. Fetching the article page
            // 2. Extracting the main content
            // 3. Cleaning and formatting the content
            
            // For now, just mark as having full content
            article.HasFullContent = true;
            article.FullContentFetchedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;
            
            return await _repository.UpdateAsync(article);
        }

        /// <summary>
        /// Search for articles with matching text in title or content
        /// </summary>
        /// <param name="searchText">Text to search for</param>
        /// <param name="feedId">Optional feed ID to limit search scope</param>
        /// <returns>Articles that match the search criteria</returns>
        public async Task<IEnumerable<Article>> SearchArticlesAsync(string searchText, int? feedId = null)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                throw new ArgumentException("Search text cannot be empty", nameof(searchText));

            if (feedId.HasValue)
            {
                // Search within a specific feed
                return await _repository.FindAsync(
                    a => a.FeedId == feedId.Value && 
                        (a.Title.Contains(searchText) || 
                         a.Content != null && a.Content.Contains(searchText) ||
                         a.Summary.Contains(searchText))
                );
            }
            else
            {
                // Search across all feeds
                return await _repository.FindAsync(
                    a => a.Title.Contains(searchText) || 
                         a.Content != null && a.Content.Contains(searchText) ||
                         a.Summary.Contains(searchText)
                );
            }
        }
    }
}
