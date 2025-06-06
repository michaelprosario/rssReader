using AppCore.Models.Tags;
using System;
using System.Collections.Generic;

namespace AppCore.Models.Bookmarks
{
    /// <summary>
    /// Represents a bookmarked article
    /// </summary>
    public class Bookmark : BaseEntity
    {
        /// <summary>
        /// Title of the bookmark (can be customized by the user)
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Notes added by the user for this bookmark
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// ID of the article that is bookmarked
        /// </summary>
        public int ArticleId { get; set; }

        /// <summary>
        /// Article that is bookmarked
        /// </summary>
        public virtual Article Article { get; set; } = null!;

        /// <summary>
        /// Tags associated with this bookmark
        /// </summary>
        public virtual ICollection<BookmarkTag> BookmarkTags { get; set; } = new List<BookmarkTag>();

        /// <summary>
        /// When the bookmark was last accessed
        /// </summary>
        public DateTime? LastAccessedAt { get; set; }
    }
}
