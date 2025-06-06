using System;

namespace AppCore.Models
{
    /// <summary>
    /// Represents an entry in a user's reading history
    /// </summary>
    public class ReadingHistory : BaseEntity
    {
        /// <summary>
        /// ID of the article that was read
        /// </summary>
        public int ArticleId { get; set; }

        /// <summary>
        /// Article that was read
        /// </summary>
        public virtual Article Article { get; set; } = null!;

        /// <summary>
        /// When the article was opened
        /// </summary>
        public DateTime OpenedAt { get; set; }

        /// <summary>
        /// When the article was finished (if applicable)
        /// </summary>
        public DateTime? FinishedAt { get; set; }

        /// <summary>
        /// Time spent reading (in seconds)
        /// </summary>
        public int TimeSpentSeconds { get; set; }

        /// <summary>
        /// Calculated reading progress (0-100)
        /// </summary>
        public int ReadingProgress { get; set; }

        /// <summary>
        /// Whether the article was read completely
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}
