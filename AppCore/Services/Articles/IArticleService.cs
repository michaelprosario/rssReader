using AppCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Services.Articles
{
    /// <summary>
    /// Interface for Article service operations
    /// </summary>
    public interface IArticleService : IService<Article>
    {
        /// <summary>
        /// Get all unread articles
        /// </summary>
        /// <returns>List of unread articles</returns>
        Task<IEnumerable<Article>> GetUnreadArticlesAsync();

        /// <summary>
        /// Get all articles for a specific feed
        /// </summary>
        /// <param name="feedId">ID of the feed</param>
        /// <returns>Articles for the specified feed</returns>
        Task<IEnumerable<Article>> GetArticlesByFeedAsync(Guid feedId);

        /// <summary>
        /// Mark an article as read
        /// </summary>
        /// <param name="articleId">ID of the article</param>
        /// <returns>The updated article</returns>
        Task<Article?> MarkAsReadAsync(Guid articleId);

        /// <summary>
        /// Mark an article as unread
        /// </summary>
        /// <param name="articleId">ID of the article</param>
        /// <returns>The updated article</returns>
        Task<Article?> MarkAsUnreadAsync(Guid articleId);

        /// <summary>
        /// Fetch full content for an article
        /// </summary>
        /// <param name="articleId">ID of the article</param>
        /// <returns>Article with full content</returns>
        Task<Article?> FetchFullContentAsync(Guid articleId);

        /// <summary>
        /// Search for articles with matching text in title or content
        /// </summary>
        /// <param name="searchText">Text to search for</param>
        /// <param name="feedId">Optional feed ID to limit search scope</param>
        /// <returns>Articles that match the search criteria</returns>
        Task<IEnumerable<Article>> SearchArticlesAsync(string searchText, Guid? feedId = null);
    }
}
