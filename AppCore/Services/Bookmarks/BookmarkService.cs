using AppCore.Models;
using AppCore.Models.Bookmarks;
using AppCore.Models.Tags;
using AppCore.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Services.Bookmarks
{
    /// <summary>
    /// Implementation of the Bookmark service
    /// </summary>
    public class BookmarkService : ServiceBase<Bookmark>, IBookmarkService
    {
        private readonly IRepository<Article> _articleRepository;
        private readonly IRepository<Tag> _tagRepository;
        private readonly IRepository<BookmarkTag> _bookmarkTagRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bookmarkRepository">Repository for bookmark data access</param>
        /// <param name="articleRepository">Repository for article data access</param>
        /// <param name="tagRepository">Repository for tag data access</param>
        /// <param name="bookmarkTagRepository">Repository for bookmark-tag relation data access</param>
        /// <param name="validator">Validator for bookmark entities</param>
        public BookmarkService(
            IRepository<Bookmark> bookmarkRepository,
            IRepository<Article> articleRepository,
            IRepository<Tag> tagRepository,
            IRepository<BookmarkTag> bookmarkTagRepository,
            IValidator<Bookmark>? validator = null)
            : base(bookmarkRepository, validator)
        {
            _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _bookmarkTagRepository = bookmarkTagRepository ?? throw new ArgumentNullException(nameof(bookmarkTagRepository));
        }

        /// <summary>
        /// Get a bookmark by article ID
        /// </summary>
        /// <param name="articleId">ID of the article</param>
        /// <returns>The bookmark if found, otherwise null</returns>
        public async Task<Bookmark?> GetByArticleIdAsync(Guid articleId)
        {
            var bookmarks = await _repository.FindAsync(b => b.ArticleId == articleId);
            return bookmarks.FirstOrDefault();
        }

        /// <summary>
        /// Bookmark an article
        /// </summary>
        /// <param name="articleId">ID of the article to bookmark</param>
        /// <param name="notes">Optional notes for the bookmark</param>
        /// <returns>The created bookmark</returns>
        public async Task<Bookmark?> BookmarkArticleAsync(Guid articleId, string? notes = null)
        {

            // Check if article exists
            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
                throw new KeyNotFoundException($"Article with ID {articleId} not found");

            // Check if already bookmarked
            var existingBookmark = await GetByArticleIdAsync(articleId);
            if (existingBookmark != null)
                return existingBookmark;

            // Create new bookmark
            var bookmark = new Bookmark
            {
                ArticleId = articleId,
                Title = article.Title,
                Notes = notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            return await AddAsync(bookmark);
        }

        /// <summary>
        /// Add tags to a bookmark
        /// </summary>
        /// <param name="bookmarkId">ID of the bookmark</param>
        /// <param name="tagIds">IDs of the tags to add</param>
        /// <returns>The updated bookmark with tags</returns>
        public async Task<Bookmark?> AddTagsToBookmarkAsync(Guid bookmarkId, IEnumerable<Guid> tagIds)
        {

            var bookmark = await _repository.GetByIdAsync(bookmarkId);
            if (bookmark == null)
                return null;

            foreach (var tagId in tagIds)
            {
                // Check if tag exists
                var tag = await _tagRepository.GetByIdAsync(tagId);
                if (tag == null)
                    continue;

                // Check if relation already exists
                var existingRelation = (await _bookmarkTagRepository.FindAsync(bt => bt.BookmarkId == bookmarkId && bt.TagId == tagId)).FirstOrDefault();
                if (existingRelation != null)
                    continue;

                // Create new relation
                var bookmarkTag = new BookmarkTag
                {
                    BookmarkId = bookmarkId,
                    TagId = tagId
                };

                await _bookmarkTagRepository.AddAsync(bookmarkTag);
            }

            // Refresh the bookmark with updated relations
            return await _repository.GetByIdAsync(bookmarkId);
        }

        /// <summary>
        /// Remove tags from a bookmark
        /// </summary>
        /// <param name="bookmarkId">ID of the bookmark</param>
        /// <param name="tagIds">IDs of the tags to remove</param>
        /// <returns>The updated bookmark</returns>
        public async Task<Bookmark?> RemoveTagsFromBookmarkAsync(Guid bookmarkId, IEnumerable<Guid> tagIds)
        {
            var bookmark = await _repository.GetByIdAsync(bookmarkId);
            if (bookmark == null)
                return null;

            foreach (var tagId in tagIds)
            {
                // Find the relation
                var relations = await _bookmarkTagRepository.FindAsync(bt => bt.BookmarkId == bookmarkId && bt.TagId == tagId);
                foreach (var relation in relations)
                {
                    await _bookmarkTagRepository.DeleteAsync(relation);
                }
            }

            // Refresh the bookmark with updated relations
            return await _repository.GetByIdAsync(bookmarkId);
        }

        /// <summary>
        /// Get bookmarks by tag ID
        /// </summary>
        /// <param name="tagId">ID of the tag</param>
        /// <returns>Bookmarks with the specified tag</returns>
        public async Task<IEnumerable<Bookmark>> GetBookmarksByTagAsync(Guid tagId)
        {

            // Check if tag exists
            var tag = await _tagRepository.GetByIdAsync(tagId);
            if (tag == null)
                throw new KeyNotFoundException($"Tag with ID {tagId} not found");

            // Get bookmark-tag relations for this tag
            var relations = await _bookmarkTagRepository.FindAsync(bt => bt.TagId == tagId);
            var bookmarkIds = relations.Select(r => r.BookmarkId).ToList();

            if (!bookmarkIds.Any())
                return Enumerable.Empty<Bookmark>();

            // Get all bookmarks for these IDs
            var bookmarks = new List<Bookmark>();
            foreach (var id in bookmarkIds)
            {
                var bookmark = await _repository.GetByIdAsync(id);
                if (bookmark != null)
                    bookmarks.Add(bookmark);
            }

            return bookmarks;
        }

        /// <summary>
        /// Export a bookmark as markdown
        /// </summary>
        /// <param name="bookmarkId">ID of the bookmark to export</param>
        /// <param name="includeMetadata">Whether to include metadata in the export</param>
        /// <returns>The bookmark as markdown text</returns>
        public async Task<string> ExportAsMarkdownAsync(Guid bookmarkId, bool includeMetadata = true)
        {
            var bookmark = await _repository.GetByIdAsync(bookmarkId);
            if (bookmark == null)
                throw new KeyNotFoundException($"Bookmark with ID {bookmarkId} not found");

            var article = await _articleRepository.GetByIdAsync(bookmark.ArticleId);
            if (article == null)
                throw new KeyNotFoundException($"Article with ID {bookmark.ArticleId} not found");

            var sb = new StringBuilder();

            // Title
            sb.AppendLine($"# {bookmark.Title}");
            sb.AppendLine();

            // Metadata
            if (includeMetadata)
            {
                sb.AppendLine($"**Source**: [{article.Feed?.Title ?? "Unknown"}]({article.Url})");
                sb.AppendLine($"**Date**: {article.PublishedAt:yyyy-MM-dd}");
                
                if (!string.IsNullOrEmpty(article.Author))
                {
                    sb.AppendLine($"**Author**: {article.Author}");
                }

                // Add tags if available
                var relations = await _bookmarkTagRepository.FindAsync(bt => bt.BookmarkId == bookmarkId);
                if (relations.Any())
                {
                    sb.Append("**Tags**: ");
                    var tags = new List<string>();
                    foreach (var relation in relations)
                    {
                        var tag = await _tagRepository.GetByIdAsync(relation.TagId);
                        if (tag != null)
                            tags.Add(tag.Name);
                    }
                    sb.AppendLine(string.Join(", ", tags));
                }

                sb.AppendLine();
            }

            // Notes
            if (!string.IsNullOrEmpty(bookmark.Notes))
            {
                sb.AppendLine("## Notes");
                sb.AppendLine();
                sb.AppendLine(bookmark.Notes);
                sb.AppendLine();
            }

            // Content
            sb.AppendLine("## Content");
            sb.AppendLine();
            
            if (!string.IsNullOrEmpty(article.Content))
            {
                // TODO: Convert HTML to Markdown
                // For now, just include the content as is (would be HTML)
                sb.AppendLine(article.Content);
            }
            else
            {
                sb.AppendLine(article.Summary);
            }

            return sb.ToString();
        }
    }
}
