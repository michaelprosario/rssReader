using System.ComponentModel.DataAnnotations;

namespace BlogReaderApp.Models
{
    /// <summary>
    /// View model for adding a new feed
    /// </summary>
    public class FeedViewModel
    {
        /// <summary>
        /// URL of the feed (RSS, Atom, or JSON)
        /// </summary>
        [Required(ErrorMessage = "Feed URL is required")]
        [Url(ErrorMessage = "Must be a valid URL")]
        [StringLength(2048, ErrorMessage = "URL cannot exceed 2048 characters")]
        public string FeedUrl { get; set; } = string.Empty;
    }
}
