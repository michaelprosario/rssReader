using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using rssReader.Models;

namespace rssReader.Services
{
    /// <summary>
    /// Interface for tag management operations.
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        /// Gets all tags.
        /// </summary>
        Task<List<Tag>> GetAllTagsAsync();
        
        /// <summary>
        /// Gets a tag by ID.
        /// </summary>
        Task<Tag> GetTagByIdAsync(string id);
        
        /// <summary>
        /// Gets a tag by name.
        /// </summary>
        Task<Tag> GetTagByNameAsync(string name);
        
        /// <summary>
        /// Creates a new tag.
        /// </summary>
        Task<Tag> CreateTagAsync(Tag tag);
        
        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        Task<Tag> UpdateTagAsync(Tag tag);
        
        /// <summary>
        /// Deletes a tag.
        /// </summary>
        Task DeleteTagAsync(string id);
        
        /// <summary>
        /// Gets the tags for an article.
        /// </summary>
        Task<List<Tag>> GetTagsByArticleIdAsync(string articleId);
        
        /// <summary>
        /// Adds a tag to an article.
        /// </summary>
        Task AddTagToArticleAsync(string tagId, string articleId);
        
        /// <summary>
        /// Removes a tag from an article.
        /// </summary>
        Task RemoveTagFromArticleAsync(string tagId, string articleId);
        
        /// <summary>
        /// Gets the count of articles with a specific tag
        /// </summary>
        Task<int> GetArticleCountByTagIdAsync(string tagId);
        
        /// <summary>
        /// Gets all articles with a specific tag
        /// </summary>
        Task<List<Article>> GetArticlesByTagIdAsync(string tagId);
    }

    /// <summary>
    /// Implementation of tag service.
    /// </summary>
    public class TagService : ITagService
    {
        private readonly IDataStorageService _dataStorage;
        private readonly IArticleService _articleService;

        private const string TAGS_FILE = "tags.json";

        /// <summary>
        /// Constructor for TagService.
        /// </summary>
        public TagService(IDataStorageService dataStorage, IArticleService articleService)
        {
            _dataStorage = dataStorage;
            _articleService = articleService;
        }

        /// <inheritdoc/>
        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _dataStorage.LoadDataAsync<List<Tag>>(TAGS_FILE) ?? new List<Tag>();
        }

        /// <inheritdoc/>
        public async Task<Tag> GetTagByIdAsync(string id)
        {
            var tags = await GetAllTagsAsync();
            return tags.FirstOrDefault(t => t.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Tag> GetTagByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var tags = await GetAllTagsAsync();
            return tags.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (string.IsNullOrWhiteSpace(tag.Name))
            {
                throw new ArgumentException("Tag name is required");
            }

            var tags = await GetAllTagsAsync();

            // Check for duplicate names
            if (tags.Any(t => t.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A tag with the name '{tag.Name}' already exists");
            }

            // Set defaults
            if (string.IsNullOrWhiteSpace(tag.Id))
            {
                tag.Id = Guid.NewGuid().ToString();
            }

            tag.DateCreated = DateTime.Now;

            tags.Add(tag);
            await _dataStorage.SaveDataAsync(TAGS_FILE, tags);

            return tag;
        }

        /// <inheritdoc/>
        public async Task<Tag> UpdateTagAsync(Tag tag)
        {
            if (tag == null || string.IsNullOrWhiteSpace(tag.Id))
            {
                throw new ArgumentException("Tag ID is required");
            }

            var tags = await GetAllTagsAsync();
            var existingTag = tags.FirstOrDefault(t => t.Id == tag.Id);

            if (existingTag == null)
            {
                throw new InvalidOperationException($"Tag with ID {tag.Id} not found");
            }

            // Check for duplicate name (excluding this tag)
            if (tags.Any(t => t.Id != tag.Id && t.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Another tag with the name '{tag.Name}' already exists");
            }

            // Update properties
            existingTag.Name = tag.Name;
            existingTag.Description = tag.Description;
            existingTag.Color = tag.Color;

            await _dataStorage.SaveDataAsync(TAGS_FILE, tags);
            return existingTag;
        }

        /// <inheritdoc/>
        public async Task DeleteTagAsync(string id)
        {
            var tags = await GetAllTagsAsync();
            var tag = tags.FirstOrDefault(t => t.Id == id);

            if (tag == null)
            {
                return;
            }

            tags.Remove(tag);
            await _dataStorage.SaveDataAsync(TAGS_FILE, tags);

            // Remove tag from all articles
            var articles = await _articleService.GetArticlesAsync();
            bool updatedArticles = false;

            foreach (var article in articles)
            {
                if (article.Tags != null && article.Tags.Contains(id))
                {
                    article.Tags.Remove(id);
                    updatedArticles = true;
                }
            }

            if (updatedArticles)
            {
                await _dataStorage.SaveDataAsync("articles.json", articles);
            }
        }

        /// <inheritdoc/>
        public async Task<Tag> MergeTagsAsync(string sourceTagId, string targetTagId)
        {
            if (sourceTagId == targetTagId)
            {
                throw new InvalidOperationException("Source and target tags must be different");
            }

            var tags = await GetAllTagsAsync();
            var sourceTag = tags.FirstOrDefault(t => t.Id == sourceTagId);
            var targetTag = tags.FirstOrDefault(t => t.Id == targetTagId);

            if (sourceTag == null)
            {
                throw new InvalidOperationException($"Source tag with ID {sourceTagId} not found");
            }

            if (targetTag == null)
            {
                throw new InvalidOperationException($"Target tag with ID {targetTagId} not found");
            }

            // Update all articles with the source tag to use the target tag
            var articles = await _articleService.GetArticlesAsync();
            bool updatedArticles = false;

            foreach (var article in articles)
            {
                if (article.Tags != null && article.Tags.Contains(sourceTagId))
                {
                    article.Tags.Remove(sourceTagId);
                    if (!article.Tags.Contains(targetTagId))
                    {
                        article.Tags.Add(targetTagId);
                    }
                    updatedArticles = true;
                }
            }

            // Remove the source tag
            tags.Remove(sourceTag);
            await _dataStorage.SaveDataAsync(TAGS_FILE, tags);

            if (updatedArticles)
            {
                await _dataStorage.SaveDataAsync("articles.json", articles);
            }

            return targetTag;
        }

        /// <inheritdoc/>
        public async Task<List<Article>> GetArticlesByTagAsync(string tagId)
        {
            var tag = await GetTagByIdAsync(tagId);
            if (tag == null)
            {
                throw new InvalidOperationException($"Tag with ID {tagId} not found");
            }

            var allArticles = await _articleService.GetArticlesAsync();
            return allArticles.Where(a => a.Tags != null && a.Tags.Contains(tagId))
                .OrderByDescending(a => a.PublishDate)
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<List<Tag>> GetTagsWithCountsAsync()
        {
            var tags = await GetAllTagsAsync();
            var articles = await _articleService.GetArticlesAsync();

            // Count articles for each tag
            foreach (var tag in tags)
            {
                tag.ArticleCount = articles.Count(a => a.Tags != null && a.Tags.Contains(tag.Id));
            }

            // Sort by count then by name
            return tags.OrderByDescending(t => t.ArticleCount).ThenBy(t => t.Name).ToList();
        }

        /// <inheritdoc/>
        public async Task<List<Tag>> GetTagsByArticleIdAsync(string articleId)
        {
            var tags = await GetAllTagsAsync();
            var article = await _articleService.GetArticleByIdAsync(articleId);

            if (article == null || article.Tags == null)
            {
                return new List<Tag>();
            }

            return tags.Where(t => article.Tags.Contains(t.Id)).ToList();
        }

        /// <inheritdoc/>
        public async Task AddTagToArticleAsync(string tagId, string articleId)
        {
            var article = await _articleService.GetArticleByIdAsync(articleId);

            if (article == null)
            {
                throw new InvalidOperationException($"Article with ID {articleId} not found");
            }

            if (article.Tags == null)
            {
                article.Tags = new List<string>();
            }

            if (!article.Tags.Contains(tagId))
            {
                article.Tags.Add(tagId);
                await _dataStorage.SaveDataAsync("articles.json", article);
            }
        }

        /// <inheritdoc/>
        public async Task RemoveTagFromArticleAsync(string tagId, string articleId)
        {
            var article = await _articleService.GetArticleByIdAsync(articleId);

            if (article == null)
            {
                throw new InvalidOperationException($"Article with ID {articleId} not found");
            }

            if (article.Tags != null && article.Tags.Contains(tagId))
            {
                article.Tags.Remove(tagId);
                await _dataStorage.SaveDataAsync("articles.json", article);
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetArticleCountByTagIdAsync(string tagId)
        {
            var articles = await _articleService.GetArticlesAsync();
            return articles.Count(a => a.Tags != null && a.Tags.Contains(tagId));
        }

        /// <inheritdoc/>
        public async Task<List<Article>> GetArticlesByTagIdAsync(string tagId)
        {
            var tag = await GetTagByIdAsync(tagId);
            if (tag == null)
            {
                throw new InvalidOperationException($"Tag with ID {tagId} not found");
            }

            var allArticles = await _articleService.GetArticlesAsync();
            return allArticles.Where(a => a.Tags != null && a.Tags.Contains(tagId))
                .OrderByDescending(a => a.PublishDate)
                .ToList();
        }
    }
}
