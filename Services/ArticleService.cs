using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using rssReader.Models;

namespace rssReader.Services
{
    /// <summary>
    /// Interface for article management operations.
    /// </summary>
    public interface IArticleService
    {
        /// <summary>
        /// Gets all articles, optionally filtered.
        /// </summary>
        Task<List<Article>> GetArticlesAsync(string feedId = null, bool unreadOnly = false, bool bookmarkedOnly = false);
        
        /// <summary>
        /// Gets an article by ID.
        /// </summary>
        Task<Article> GetArticleByIdAsync(string id);
        
        /// <summary>
        /// Gets unread count for a feed.
        /// </summary>
        Task<int> GetUnreadCountAsync(string feedId);
        
        /// <summary>
        /// Adds new articles, avoiding duplicates.
        /// </summary>
        Task<int> AddArticlesAsync(List<Article> articles, string feedId);
        
        /// <summary>
        /// Updates an article.
        /// </summary>
        Task<Article> UpdateArticleAsync(Article article);
        
        /// <summary>
        /// Marks an article as read/unread.
        /// </summary>
        Task<Article> MarkArticleAsReadAsync(string id, bool isRead);
        
        /// <summary>
        /// Marks all articles in a feed as read.
        /// </summary>
        Task<int> MarkAllAsReadAsync(string feedId = null);
        
        /// <summary>
        /// Bookmarks/unbookmarks an article.
        /// </summary>
        Task<Article> BookmarkArticleAsync(string id, bool isBookmarked);
        
        /// <summary>
        /// Gets all bookmarked articles.
        /// </summary>
        Task<List<Article>> GetBookmarkedArticlesAsync();
        
        /// <summary>
        /// Fetches the full content for an article.
        /// </summary>
        Task<Article> FetchFullContentAsync(string id);
        
        /// <summary>
        /// Searches articles by query.
        /// </summary>
        Task<List<Article>> SearchArticlesAsync(string query, string feedId = null);
        
        /// <summary>
        /// Adds tags to an article.
        /// </summary>
        Task<Article> AddTagsToArticleAsync(string id, List<string> tagIds);
        
        /// <summary>
        /// Removes tags from an article.
        /// </summary>
        Task<Article> RemoveTagFromArticleAsync(string id, string tagId);
        
        /// <summary>
        /// Deletes articles for a feed.
        /// </summary>
        Task<int> DeleteArticlesByFeedIdAsync(string feedId);
        
        /// <summary>
        /// Exports bookmarked articles as markdown.
        /// </summary>
        Task<string> ExportBookmarksAsMarkdownAsync(bool includeContent = true, bool includeTags = true);
    }

    /// <summary>
    /// Implementation of article service.
    /// </summary>
    public class ArticleService : IArticleService
    {
        private readonly IDataStorageService _dataStorage;
        private readonly HttpClient _httpClient;

        private const string ARTICLES_FILE = "articles.json";

        /// <summary>
        /// Constructor for ArticleService.
        /// </summary>
        public ArticleService(IDataStorageService dataStorage, HttpClient httpClient)
        {
            _dataStorage = dataStorage;
            _httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<List<Article>> GetArticlesAsync(string feedId = null, bool unreadOnly = false, bool bookmarkedOnly = false)
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(feedId))
            {
                articles = articles.Where(a => a.FeedId == feedId).ToList();
            }
            
            if (unreadOnly)
            {
                articles = articles.Where(a => !a.IsRead).ToList();
            }
            
            if (bookmarkedOnly)
            {
                articles = articles.Where(a => a.IsBookmarked).ToList();
            }
            
            // Sort by publish date, newest first
            return articles.OrderByDescending(a => a.PublishDate).ToList();
        }

        /// <inheritdoc/>
        public async Task<Article> GetArticleByIdAsync(string id)
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            return articles.FirstOrDefault(a => a.Id == id);
        }

        /// <inheritdoc/>
        public async Task<int> GetUnreadCountAsync(string feedId)
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            return articles.Count(a => a.FeedId == feedId && !a.IsRead);
        }

        /// <inheritdoc/>
        public async Task<int> AddArticlesAsync(List<Article> articles, string feedId)
        {
            if (articles == null || !articles.Any())
            {
                return 0;
            }
            
            var existingArticles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            
            // Keep track of how many new articles were added
            int addedCount = 0;
            
            foreach (var article in articles)
            {
                // Set the feed ID if not already set
                if (string.IsNullOrWhiteSpace(article.FeedId))
                {
                    article.FeedId = feedId;
                }
                
                // Check if article already exists (by URL or original ID)
                if (!existingArticles.Any(a => 
                    (a.FeedId == article.FeedId && a.OriginalId == article.OriginalId) || 
                    (!string.IsNullOrWhiteSpace(article.Url) && a.Url == article.Url)))
                {
                    // Set a new ID and add the article
                    article.Id = Guid.NewGuid().ToString();
                    article.DiscoveryDate = DateTime.Now;
                    
                    existingArticles.Add(article);
                    addedCount++;
                }
            }
            
            if (addedCount > 0)
            {
                await _dataStorage.SaveDataAsync(ARTICLES_FILE, existingArticles);
            }
            
            return addedCount;
        }

        /// <inheritdoc/>
        public async Task<Article> UpdateArticleAsync(Article article)
        {
            if (article == null || string.IsNullOrWhiteSpace(article.Id))
            {
                throw new ArgumentException("Article ID is required");
            }
            
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            var existingArticle = articles.FirstOrDefault(a => a.Id == article.Id);
            
            if (existingArticle == null)
            {
                throw new InvalidOperationException($"Article with ID {article.Id} not found");
            }
            
            // Update properties
            existingArticle.Title = article.Title;
            existingArticle.Summary = article.Summary;
            existingArticle.Content = article.Content;
            existingArticle.Url = article.Url;
            existingArticle.IsRead = article.IsRead;
            existingArticle.IsBookmarked = article.IsBookmarked;
            existingArticle.IsFullContentFetched = article.IsFullContentFetched;
            existingArticle.Tags = article.Tags;
            existingArticle.ImageUrl = article.ImageUrl;
            
            // Update timestamps if applicable
            if (article.IsRead && !existingArticle.ReadDate.HasValue)
            {
                existingArticle.ReadDate = DateTime.Now;
            }
            else if (!article.IsRead)
            {
                existingArticle.ReadDate = null;
            }
            
            if (article.IsBookmarked && !existingArticle.BookmarkDate.HasValue)
            {
                existingArticle.BookmarkDate = DateTime.Now;
            }
            else if (!article.IsBookmarked)
            {
                existingArticle.BookmarkDate = null;
            }
            
            await _dataStorage.SaveDataAsync(ARTICLES_FILE, articles);
            return existingArticle;
        }

        /// <inheritdoc/>
        public async Task<Article> MarkArticleAsReadAsync(string id, bool isRead)
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            var article = articles.FirstOrDefault(a => a.Id == id);
            
            if (article == null)
            {
                throw new InvalidOperationException($"Article with ID {id} not found");
            }
            
            article.IsRead = isRead;
            article.ReadDate = isRead ? DateTime.Now : (DateTime?)null;
            
            await _dataStorage.SaveDataAsync(ARTICLES_FILE, articles);
            return article;
        }

        /// <inheritdoc/>
        public async Task<int> MarkAllAsReadAsync(string feedId = null)
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            var articlesToMark = articles.Where(a => !a.IsRead && (string.IsNullOrWhiteSpace(feedId) || a.FeedId == feedId)).ToList();
            
            foreach (var article in articlesToMark)
            {
                article.IsRead = true;
                article.ReadDate = DateTime.Now;
            }
            
            await _dataStorage.SaveDataAsync(ARTICLES_FILE, articles);
            return articlesToMark.Count;
        }

        /// <inheritdoc/>
        public async Task<Article> BookmarkArticleAsync(string id, bool isBookmarked)
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            var article = articles.FirstOrDefault(a => a.Id == id);
            
            if (article == null)
            {
                throw new InvalidOperationException($"Article with ID {id} not found");
            }
            
            article.IsBookmarked = isBookmarked;
            article.BookmarkDate = isBookmarked ? DateTime.Now : (DateTime?)null;
            
            await _dataStorage.SaveDataAsync(ARTICLES_FILE, articles);
            return article;
        }

        /// <inheritdoc/>
        public async Task<List<Article>> GetBookmarkedArticlesAsync()
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            return articles.Where(a => a.IsBookmarked).OrderByDescending(a => a.BookmarkDate).ToList();
        }

        /// <inheritdoc/>
        public async Task<Article> FetchFullContentAsync(string id)
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            var article = articles.FirstOrDefault(a => a.Id == id);
            
            if (article == null)
            {
                throw new InvalidOperationException($"Article with ID {id} not found");
            }
            
            if (string.IsNullOrWhiteSpace(article.Url))
            {
                throw new InvalidOperationException("Article URL is missing");
            }
            
            try
            {
                var response = await _httpClient.GetAsync(article.Url);
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync();
                
                // Parse with HtmlAgilityPack
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                
                // Try different common selectors for article content
                var contentNode = doc.DocumentNode.SelectSingleNode("//article") ??
                                 doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'content')]") ??
                                 doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'post-content')]") ??
                                 doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'entry-content')]") ??
                                 doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'article-content')]");
                
                if (contentNode != null)
                {
                    // Clean up content (remove scripts, etc.)
                    var scriptsToRemove = contentNode.SelectNodes("//script|//style|//iframe");
                    if (scriptsToRemove != null)
                    {
                        foreach (var node in scriptsToRemove)
                        {
                            node.Remove();
                        }
                    }
                    
                    article.Content = contentNode.InnerHtml;
                    article.IsFullContentFetched = true;
                    
                    await _dataStorage.SaveDataAsync(ARTICLES_FILE, articles);
                }
                
                return article;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to fetch full content: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<List<Article>> SearchArticlesAsync(string query, string feedId = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Article>();
            }
            
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            
            // Filter by feed if specified
            if (!string.IsNullOrWhiteSpace(feedId))
            {
                articles = articles.Where(a => a.FeedId == feedId).ToList();
            }
            
            // Split query into terms for better matching
            var terms = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            // Match articles that contain all terms in title, summary, or content
            var results = articles.Where(a => 
                terms.All(term => 
                    (a.Title?.ToLowerInvariant().Contains(term) ?? false) || 
                    (a.Summary?.ToLowerInvariant().Contains(term) ?? false) || 
                    (a.Content?.ToLowerInvariant().Contains(term) ?? false)
                )
            ).ToList();
            
            // Sort by relevance (number of matches) and then by date
            return results.OrderByDescending(a => 
                terms.Sum(term => 
                    (a.Title?.ToLowerInvariant().Split(term).Length - 1 ?? 0) +
                    (a.Summary?.ToLowerInvariant().Split(term).Length - 1 ?? 0) +
                    (a.Content?.ToLowerInvariant().Split(term).Length - 1 ?? 0)
                )
            ).ThenByDescending(a => a.PublishDate).ToList();
        }

        /// <inheritdoc/>
        public async Task<Article> AddTagsToArticleAsync(string id, List<string> tagIds)
        {
            if (tagIds == null || !tagIds.Any())
            {
                return await GetArticleByIdAsync(id);
            }
            
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            var article = articles.FirstOrDefault(a => a.Id == id);
            
            if (article == null)
            {
                throw new InvalidOperationException($"Article with ID {id} not found");
            }
            
            // Initialize tags list if null
            if (article.Tags == null)
            {
                article.Tags = new List<string>();
            }
            
            // Add new tags
            foreach (var tagId in tagIds)
            {
                if (!article.Tags.Contains(tagId))
                {
                    article.Tags.Add(tagId);
                }
            }
            
            await _dataStorage.SaveDataAsync(ARTICLES_FILE, articles);
            return article;
        }

        /// <inheritdoc/>
        public async Task<Article> RemoveTagFromArticleAsync(string id, string tagId)
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            var article = articles.FirstOrDefault(a => a.Id == id);
            
            if (article == null)
            {
                throw new InvalidOperationException($"Article with ID {id} not found");
            }
            
            if (article.Tags != null && article.Tags.Contains(tagId))
            {
                article.Tags.Remove(tagId);
                await _dataStorage.SaveDataAsync(ARTICLES_FILE, articles);
            }
            
            return article;
        }

        /// <inheritdoc/>
        public async Task<int> DeleteArticlesByFeedIdAsync(string feedId)
        {
            var articles = await _dataStorage.LoadDataAsync<List<Article>>(ARTICLES_FILE) ?? new List<Article>();
            var articlesToRemove = articles.Where(a => a.FeedId == feedId).ToList();
            
            foreach (var article in articlesToRemove)
            {
                articles.Remove(article);
            }
            
            await _dataStorage.SaveDataAsync(ARTICLES_FILE, articles);
            return articlesToRemove.Count;
        }

        /// <inheritdoc/>
        public async Task<string> ExportBookmarksAsMarkdownAsync(bool includeContent = true, bool includeTags = true)
        {
            var bookmarks = await GetBookmarkedArticlesAsync();
            
            if (!bookmarks.Any())
            {
                return "# Bookmarks\n\nNo bookmarked articles found.";
            }
            
            var markdown = "# Bookmarks\n\n";
            markdown += $"Exported on {DateTime.Now:yyyy-MM-dd HH:mm}\n\n";
            
            foreach (var bookmark in bookmarks)
            {
                markdown += $"## {bookmark.Title}\n\n";
                
                if (bookmark.Authors != null && bookmark.Authors.Any())
                {
                    markdown += $"By {string.Join(", ", bookmark.Authors)}\n\n";
                }
                
                markdown += $"Date: {bookmark.PublishDate:yyyy-MM-dd HH:mm}\n\n";
                
                if (!string.IsNullOrWhiteSpace(bookmark.Url))
                {
                    markdown += $"[Original Link]({bookmark.Url})\n\n";
                }
                
                if (includeTags && bookmark.Tags != null && bookmark.Tags.Any())
                {
                    markdown += $"Tags: {string.Join(", ", bookmark.Tags)}\n\n";
                }
                
                if (!string.IsNullOrWhiteSpace(bookmark.Summary))
                {
                    markdown += $"{bookmark.Summary}\n\n";
                }
                
                if (includeContent && !string.IsNullOrWhiteSpace(bookmark.Content))
                {
                    // Convert HTML to Markdown (very simplified)
                    var content = bookmark.Content
                        .Replace("<h1>", "# ")
                        .Replace("</h1>", "\n\n")
                        .Replace("<h2>", "## ")
                        .Replace("</h2>", "\n\n")
                        .Replace("<h3>", "### ")
                        .Replace("</h3>", "\n\n")
                        .Replace("<p>", "")
                        .Replace("</p>", "\n\n")
                        .Replace("<br>", "\n")
                        .Replace("<br/>", "\n")
                        .Replace("<br />", "\n")
                        .Replace("<strong>", "**")
                        .Replace("</strong>", "**")
                        .Replace("<b>", "**")
                        .Replace("</b>", "**")
                        .Replace("<em>", "*")
                        .Replace("</em>", "*")
                        .Replace("<i>", "*")
                        .Replace("</i>", "*");
                    
                    markdown += $"{content}\n\n";
                }
                
                markdown += "---\n\n";
            }
            
            return markdown;
        }
    }
}
