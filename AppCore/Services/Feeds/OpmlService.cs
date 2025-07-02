using AppCore.Models.Feeds;
using AppCore.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppCore.Services.Feeds
{
    /// <summary>
    /// Implementation of the OPML service
    /// </summary>
    public class OpmlService : IOpmlService
    {
        private readonly IRepository<OpmlOperation> _operationRepository;
        private readonly IRepository<Feed> _feedRepository;
        private readonly IFeedService _feedService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="operationRepository">Repository for OPML operation data access</param>
        /// <param name="feedRepository">Repository for feed data access</param>
        /// <param name="feedService">Service for feed operations</param>
        public OpmlService(
            IRepository<OpmlOperation> operationRepository,
            IRepository<Feed> feedRepository,
            IFeedService feedService)
        {
            _operationRepository = operationRepository ?? throw new ArgumentNullException(nameof(operationRepository));
            _feedRepository = feedRepository ?? throw new ArgumentNullException(nameof(feedRepository));
            _feedService = feedService ?? throw new ArgumentNullException(nameof(feedService));
        }

        /// <summary>
        /// Import feeds from OPML content
        /// </summary>
        /// <param name="opmlContent">OPML content as string</param>
        /// <param name="fileName">Name of the OPML file</param>
        /// <returns>The OPML operation record</returns>
        public async Task<OpmlOperation> ImportFromOpmlAsync(string opmlContent, string fileName)
        {
            if (string.IsNullOrWhiteSpace(opmlContent))
                throw new ArgumentException("OPML content cannot be empty", nameof(opmlContent));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty", nameof(fileName));

            // Create operation record
            var operation = new OpmlOperation
            {
                OperationType = OpmlOperationType.Import,
                FileName = fileName,
                Status = OpmlOperationStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            operation = await _operationRepository.AddAsync(operation);

            try
            {
                // Parse OPML content
                var outlines = ParseOpml(opmlContent);
                operation.TotalFeeds = outlines.Count;
                await _operationRepository.UpdateAsync(operation);

                // Process each feed
                foreach (var (title, feedUrl, websiteUrl) in outlines)
                {
                    try
                    {
                        // Check if feed already exists
                        var existingFeed = await _feedService.GetByUrlAsync(feedUrl);
                        if (existingFeed == null)
                        {
                            // Create new feed
                            var feed = new Feed
                            {
                                Title = title,
                                FeedUrl = feedUrl,
                                WebsiteUrl = websiteUrl,
                                FeedType = DetermineFeedType(feedUrl),
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            };

                            await _feedService.AddAsync(feed);
                            operation.SuccessfulFeeds++;
                        }
                        else
                        {
                            // Feed already exists, consider it successful
                            operation.SuccessfulFeeds++;
                        }
                    }
                    catch
                    {
                        // If adding a feed fails, increment the failed count
                        operation.FailedFeeds++;
                    }
                    finally
                    {
                        operation.ProcessedFeeds++;
                        await _operationRepository.UpdateAsync(operation);
                    }

                    // Check if operation was canceled
                    var refreshedOperation = await _operationRepository.GetByIdAsync(operation.Id);
                    if (refreshedOperation?.Status == OpmlOperationStatus.Cancelled)
                    {
                        break;
                    }
                }

                // Complete the operation
                operation.Status = OpmlOperationStatus.Completed;
                operation.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                operation.Status = OpmlOperationStatus.Failed;
                operation.CompletedAt = DateTime.UtcNow;
                operation.ErrorMessage = ex.Message;
            }

            return await _operationRepository.UpdateAsync(operation);
        }

        /// <summary>
        /// Export feeds to OPML format
        /// </summary>
        /// <param name="fileName">Name for the OPML file</param>
        /// <returns>OPML content as string and the operation record</returns>
        public async Task<(string OpmlContent, OpmlOperation Operation)> ExportToOpmlAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty", nameof(fileName));

            // Create operation record
            var operation = new OpmlOperation
            {
                OperationType = OpmlOperationType.Export,
                FileName = fileName,
                Status = OpmlOperationStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            operation = await _operationRepository.AddAsync(operation);

            try
            {
                // Get all feeds
                var feeds = await _feedRepository.GetAllAsync();
                operation.TotalFeeds = feeds.Count();

                // Create OPML document
                var opmlContent = GenerateOpml(feeds);

                // Complete the operation
                operation.ProcessedFeeds = feeds.Count();
                operation.SuccessfulFeeds = feeds.Count();
                operation.Status = OpmlOperationStatus.Completed;
                operation.CompletedAt = DateTime.UtcNow;
                await _operationRepository.UpdateAsync(operation);

                return (opmlContent, operation);
            }
            catch (Exception ex)
            {
                operation.Status = OpmlOperationStatus.Failed;
                operation.CompletedAt = DateTime.UtcNow;
                operation.ErrorMessage = ex.Message;
                await _operationRepository.UpdateAsync(operation);

                throw;
            }
        }

        /// <summary>
        /// Get an OPML operation by ID
        /// </summary>
        /// <param name="operationId">ID of the operation</param>
        /// <returns>The OPML operation</returns>
        public async Task<OpmlOperation?> GetOperationAsync(Guid operationId)
        {
            return await _operationRepository.GetByIdAsync(operationId);
        }

        /// <summary>
        /// Get recent OPML operations
        /// </summary>
        /// <param name="count">Maximum number of operations to return</param>
        /// <returns>List of recent OPML operations</returns>
        public async Task<IEnumerable<OpmlOperation>> GetRecentOperationsAsync(int count = 10)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than zero", nameof(count));

            var operations = await _operationRepository.GetAllAsync();
            return operations.OrderByDescending(o => o.StartedAt).Take(count);
        }

        /// <summary>
        /// Cancel an in-progress OPML operation
        /// </summary>
        /// <param name="operationId">ID of the operation to cancel</param>
        /// <returns>The canceled operation</returns>
        public async Task<OpmlOperation?> CancelOperationAsync(Guid operationId)
        {
            var operation = await _operationRepository.GetByIdAsync(operationId);
            if (operation == null)
                return null;

            if (operation.Status != OpmlOperationStatus.InProgress)
                throw new InvalidOperationException("Only in-progress operations can be cancelled");

            operation.Status = OpmlOperationStatus.Cancelled;
            operation.CompletedAt = DateTime.UtcNow;
            operation.UpdatedAt = DateTime.UtcNow;

            return await _operationRepository.UpdateAsync(operation);
        }

        /// <summary>
        /// Parse OPML content to extract feeds
        /// </summary>
        /// <param name="opmlContent">OPML content as string</param>
        /// <returns>List of feed information tuples</returns>
        private List<(string Title, string FeedUrl, string WebsiteUrl)> ParseOpml(string opmlContent)
        {
            var result = new List<(string Title, string FeedUrl, string WebsiteUrl)>();

            try
            {
                XDocument doc = XDocument.Parse(opmlContent);
                var outlines = doc.Descendants("outline");

                foreach (var outline in outlines)
                {
                    var type = outline.Attribute("type")?.Value;
                    if (string.Equals(type, "rss", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(type, "atom", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(type, "feed", StringComparison.OrdinalIgnoreCase))
                    {
                        var title = outline.Attribute("text")?.Value ?? outline.Attribute("title")?.Value ?? "Untitled Feed";
                        var feedUrl = outline.Attribute("xmlUrl")?.Value ?? "";
                        var websiteUrl = outline.Attribute("htmlUrl")?.Value ?? "";

                        if (!string.IsNullOrWhiteSpace(feedUrl))
                        {
                            result.Add((title, feedUrl, websiteUrl));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list on parse error
                // In a real application, this exception should be logged
            }

            return result;
        }

        /// <summary>
        /// Generate OPML content from feeds
        /// </summary>
        /// <param name="feeds">Collection of feeds</param>
        /// <returns>OPML content as string</returns>
        private string GenerateOpml(IEnumerable<Feed> feeds)
        {
            var doc = new XDocument(
                new XElement("opml",
                    new XAttribute("version", "1.0"),
                    new XElement("head",
                        new XElement("title", "RSS Reader Feed Export"),
                        new XElement("dateCreated", DateTime.UtcNow.ToString("r"))
                    ),
                    new XElement("body")
                )
            );

            var body = doc.Root?.Element("body");
            if (body != null)
            {
                foreach (var feed in feeds)
                {
                    body.Add(new XElement("outline",
                        new XAttribute("text", feed.Title),
                        new XAttribute("title", feed.Title),
                        new XAttribute("type", feed.FeedType.ToString().ToLower()),
                        new XAttribute("xmlUrl", feed.FeedUrl),
                        new XAttribute("htmlUrl", feed.WebsiteUrl ?? "")
                    ));
                }
            }

            return doc.ToString();
        }

        /// <summary>
        /// Determine feed type from URL
        /// </summary>
        /// <param name="feedUrl">Feed URL</param>
        /// <returns>Feed type enum value</returns>
        private FeedType DetermineFeedType(string feedUrl)
        {
            if (feedUrl.Contains(".json", StringComparison.OrdinalIgnoreCase))
            {
                return FeedType.JSON;
            }
            else if (feedUrl.Contains("atom", StringComparison.OrdinalIgnoreCase))
            {
                return FeedType.Atom;
            }
            else
            {
                return FeedType.RSS;
            }
        }
    }
}
