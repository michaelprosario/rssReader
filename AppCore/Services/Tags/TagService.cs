using AppCore.Models.Tags;
using AppCore.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppCore.Services.Tags
{
    /// <summary>
    /// Implementation of the Tag service
    /// </summary>
    public class TagService : ServiceBase<Tag>, ITagService
    {
        private readonly IRepository<BookmarkTag> _bookmarkTagRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagRepository">Repository for tag data access</param>
        /// <param name="bookmarkTagRepository">Repository for bookmark-tag relation data access</param>
        /// <param name="validator">Validator for tag entities</param>
        public TagService(
            IRepository<Tag> tagRepository,
            IRepository<BookmarkTag> bookmarkTagRepository,
            IValidator<Tag>? validator = null)
            : base(tagRepository, validator)
        {
            _bookmarkTagRepository = bookmarkTagRepository ?? throw new ArgumentNullException(nameof(bookmarkTagRepository));
        }

        /// <summary>
        /// Get a tag by name
        /// </summary>
        /// <param name="name">Tag name</param>
        /// <returns>The tag if found, otherwise null</returns>
        public async Task<Tag?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be empty", nameof(name));

            var tags = await _repository.FindAsync(t => t.Name.ToLower() == name.ToLower());
            return tags.FirstOrDefault();
        }

        /// <summary>
        /// Create a new tag if it doesn't exist
        /// </summary>
        /// <param name="name">Tag name</param>
        /// <param name="color">Optional color for the tag</param>
        /// <returns>The created or existing tag</returns>
        public async Task<Tag> GetOrCreateTagAsync(string name, string? color = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be empty", nameof(name));

            var existingTag = await GetByNameAsync(name);
            if (existingTag != null)
                return existingTag;

            var tag = new Tag
            {
                Name = name.Trim(),
                Color = color ?? "#808080", // Default gray color
                CreatedAt = DateTime.UtcNow
            };

            return await AddAsync(tag);
        }

        /// <summary>
        /// Get most used tags
        /// </summary>
        /// <param name="count">Number of tags to return</param>
        /// <returns>List of most used tags</returns>
        public async Task<IEnumerable<Tag>> GetMostUsedTagsAsync(int count = 10)
        {
            // Get all tags
            var allTags = await _repository.GetAllAsync();
            var tagCounts = new Dictionary<int, int>();

            // Count bookmarks per tag
            foreach (var tag in allTags)
            {
                var bookmarkTags = await _bookmarkTagRepository.FindAsync(bt => bt.TagId == tag.Id);
                tagCounts[tag.Id] = bookmarkTags.Count();
            }

            // Sort by count and take top N
            var topTagIds = tagCounts
                .OrderByDescending(kv => kv.Value)
                .Take(count)
                .Select(kv => kv.Key);

            // Get tag objects for the IDs
            var result = new List<Tag>();
            foreach (var id in topTagIds)
            {
                var tag = await _repository.GetByIdAsync(id);
                if (tag != null)
                    result.Add(tag);
            }

            return result;
        }

        /// <summary>
        /// Merge two tags
        /// </summary>
        /// <param name="sourceTagId">ID of the source tag</param>
        /// <param name="targetTagId">ID of the target tag</param>
        /// <returns>The merged tag</returns>
        public async Task<Tag?> MergeTagsAsync(int sourceTagId, int targetTagId)
        {
            if (sourceTagId <= 0)
                throw new ArgumentException("Source tag ID must be greater than zero", nameof(sourceTagId));

            if (targetTagId <= 0)
                throw new ArgumentException("Target tag ID must be greater than zero", nameof(targetTagId));

            if (sourceTagId == targetTagId)
                throw new ArgumentException("Source and target tags cannot be the same", nameof(targetTagId));

            // Get both tags
            var sourceTag = await _repository.GetByIdAsync(sourceTagId);
            var targetTag = await _repository.GetByIdAsync(targetTagId);

            if (sourceTag == null)
                throw new KeyNotFoundException($"Source tag with ID {sourceTagId} not found");

            if (targetTag == null)
                throw new KeyNotFoundException($"Target tag with ID {targetTagId} not found");

            // Get all bookmark-tag relations for the source tag
            var sourceRelations = await _bookmarkTagRepository.FindAsync(bt => bt.TagId == sourceTagId);

            // Move relations to target tag
            foreach (var relation in sourceRelations)
            {
                // Check if a relation already exists for this bookmark with the target tag
                var existingRelation = (await _bookmarkTagRepository.FindAsync(
                    bt => bt.BookmarkId == relation.BookmarkId && bt.TagId == targetTagId)).FirstOrDefault();

                if (existingRelation == null)
                {
                    // Create new relation with target tag
                    var newRelation = new BookmarkTag
                    {
                        BookmarkId = relation.BookmarkId,
                        TagId = targetTagId
                    };
                    await _bookmarkTagRepository.AddAsync(newRelation);
                }

                // Delete the old relation
                await _bookmarkTagRepository.DeleteAsync(relation);
            }

            // Delete the source tag
            await _repository.DeleteAsync(sourceTagId);

            // Return the target tag (which now has all the relations)
            return targetTag;
        }

        /// <summary>
        /// Get tags that match a partial name
        /// </summary>
        /// <param name="partialName">Partial tag name</param>
        /// <param name="limit">Maximum number of results</param>
        /// <returns>List of matching tags</returns>
        public async Task<IEnumerable<Tag>> GetTagSuggestionsAsync(string partialName, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(partialName))
                return Enumerable.Empty<Tag>();

            var lowercaseName = partialName.ToLower();
            var allTags = await _repository.GetAllAsync();
            
            return allTags
                .Where(t => t.Name.ToLower().Contains(lowercaseName))
                .OrderBy(t => t.Name)
                .Take(limit);
        }
    }
}
