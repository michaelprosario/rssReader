using AppCore.Models.Tags;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Services.Tags
{
    /// <summary>
    /// Interface for Tag service operations
    /// </summary>
    public interface ITagService : IService<Tag>
    {
        /// <summary>
        /// Get a tag by name
        /// </summary>
        /// <param name="name">Tag name</param>
        /// <returns>The tag if found, otherwise null</returns>
        Task<Tag?> GetByNameAsync(string name);

        /// <summary>
        /// Create a new tag if it doesn't exist
        /// </summary>
        /// <param name="name">Tag name</param>
        /// <param name="color">Optional color for the tag</param>
        /// <returns>The created or existing tag</returns>
        Task<Tag> GetOrCreateTagAsync(string name, string? color = null);

        /// <summary>
        /// Get most used tags
        /// </summary>
        /// <param name="count">Number of tags to return</param>
        /// <returns>List of most used tags</returns>
        Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int count = 10);

        /// <summary>
        /// Merge two tags
        /// </summary>
        /// <param name="sourceTagId">ID of the source tag</param>
        /// <param name="targetTagId">ID of the target tag</param>
        /// <returns>The merged tag</returns>
        Task<Tag?> MergeTagsAsync(Guid sourceTagId, Guid targetTagId);

        /// <summary>
        /// Get tags that match a partial name
        /// </summary>
        /// <param name="partialName">Partial tag name</param>
        /// <param name="limit">Maximum number of results</param>
        /// <returns>List of matching tags</returns>
        Task<IEnumerable<Tag>> GetTagSuggestionsAsync(string partialName, int limit = 10);
    }
}
