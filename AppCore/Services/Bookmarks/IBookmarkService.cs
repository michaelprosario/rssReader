using AppCore.Models.Bookmarks;
using AppCore.Models.Tags;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Services.Bookmarks
{
    /// <summary>
    /// Interface for Bookmark service operations
    /// </summary>
    public interface IBookmarkService : IService<Bookmark>
    {
        /// <summary>
        /// Get a bookmark by article ID
        /// </summary>
        /// <param name="articleId">ID of the article</param>
        /// <returns>The bookmark if found, otherwise null</returns>
        Task<Bookmark?> GetByArticleIdAsync(int articleId);

        /// <summary>
        /// Bookmark an article
        /// </summary>
        /// <param name="articleId">ID of the article to bookmark</param>
        /// <param name="notes">Optional notes for the bookmark</param>
        /// <returns>The created bookmark</returns>
        Task<Bookmark?> BookmarkArticleAsync(int articleId, string? notes = null);

        /// <summary>
        /// Add tags to a bookmark
        /// </summary>
        /// <param name="bookmarkId">ID of the bookmark</param>
        /// <param name="tagIds">IDs of the tags to add</param>
        /// <returns>The updated bookmark with tags</returns>
        Task<Bookmark?> AddTagsToBookmarkAsync(int bookmarkId, IEnumerable<int> tagIds);

        /// <summary>
        /// Remove tags from a bookmark
        /// </summary>
        /// <param name="bookmarkId">ID of the bookmark</param>
        /// <param name="tagIds">IDs of the tags to remove</param>
        /// <returns>The updated bookmark</returns>
        Task<Bookmark?> RemoveTagsFromBookmarkAsync(int bookmarkId, IEnumerable<int> tagIds);

        /// <summary>
        /// Get bookmarks by tag ID
        /// </summary>
        /// <param name="tagId">ID of the tag</param>
        /// <returns>Bookmarks with the specified tag</returns>
        Task<IEnumerable<Bookmark>> GetBookmarksByTagAsync(int tagId);

        /// <summary>
        /// Export a bookmark as markdown
        /// </summary>
        /// <param name="bookmarkId">ID of the bookmark to export</param>
        /// <param name="includeMetadata">Whether to include metadata in the export</param>
        /// <returns>The bookmark as markdown text</returns>
        Task<string> ExportAsMarkdownAsync(int bookmarkId, bool includeMetadata = true);
    }
}
