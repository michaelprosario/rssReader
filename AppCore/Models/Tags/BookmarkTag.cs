using AppCore.Models.Bookmarks;

namespace AppCore.Models.Tags
{
    /// <summary>
    /// Join entity for the many-to-many relationship between Bookmarks and Tags
    /// </summary>
    public class BookmarkTag : IEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of the bookmark
        /// </summary>
        public int BookmarkId { get; set; }

        /// <summary>
        /// Bookmark associated with this tag
        /// </summary>
        public virtual Bookmark Bookmark { get; set; } = null!;

        /// <summary>
        /// ID of the tag
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// Tag associated with this bookmark
        /// </summary>
        public virtual Tag Tag { get; set; } = null!;
    }
}
