using AppCore.Models.Bookmarks;
using System.Collections.Generic;

namespace AppCore.Models.Tags
{
    /// <summary>
    /// Represents a tag that can be applied to bookmarks
    /// </summary>
    public class Tag : BaseEntity
    {
        /// <summary>
        /// Name of the tag
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the tag
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Color code for the tag (hex format)
        /// </summary>
        public string Color { get; set; } = "#808080";

        /// <summary>
        /// Bookmarks associated with this tag
        /// </summary>
        public virtual ICollection<BookmarkTag> BookmarkTags { get; set; } = new List<BookmarkTag>();

        /// <summary>
        /// Number of bookmarks associated with this tag
        /// </summary>
        public int BookmarkCount => BookmarkTags.Count;
    }
}
