using System;

namespace AppCore.Models.Feeds
{
    /// <summary>
    /// Represents an OPML import or export operation
    /// </summary>
    public class OpmlOperation : BaseEntity
    {
        /// <summary>
        /// Type of operation (import or export)
        /// </summary>
        public OpmlOperationType OperationType { get; set; }

        /// <summary>
        /// File name for the OPML file
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Total number of feeds in the operation
        /// </summary>
        public int TotalFeeds { get; set; }

        /// <summary>
        /// Number of feeds processed so far
        /// </summary>
        public int ProcessedFeeds { get; set; }

        /// <summary>
        /// Number of feeds successfully processed
        /// </summary>
        public int SuccessfulFeeds { get; set; }

        /// <summary>
        /// Number of feeds that failed to process
        /// </summary>
        public int FailedFeeds { get; set; }

        /// <summary>
        /// Status of the operation
        /// </summary>
        public OpmlOperationStatus Status { get; set; }

        /// <summary>
        /// When the operation started
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// When the operation completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Any error message associated with the operation
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Progress of the operation (0-100)
        /// </summary>
        public int ProgressPercentage => TotalFeeds == 0 ? 0 : (ProcessedFeeds * 100) / TotalFeeds;
    }

    /// <summary>
    /// Type of OPML operation
    /// </summary>
    public enum OpmlOperationType
    {
        Import,
        Export
    }

    /// <summary>
    /// Status of an OPML operation
    /// </summary>
    public enum OpmlOperationStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }
}
