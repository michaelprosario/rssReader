using AppCore.Models.Feeds;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Services.Feeds
{
    /// <summary>
    /// Interface for OPML operations
    /// </summary>
    public interface IOpmlService
    {
        /// <summary>
        /// Import feeds from OPML content
        /// </summary>
        /// <param name="opmlContent">OPML content as string</param>
        /// <param name="fileName">Name of the OPML file</param>
        /// <returns>The OPML operation record</returns>
        Task<OpmlOperation> ImportFromOpmlAsync(string opmlContent, string fileName);

        /// <summary>
        /// Export feeds to OPML format
        /// </summary>
        /// <param name="fileName">Name for the OPML file</param>
        /// <returns>OPML content as string and the operation record</returns>
        Task<(string OpmlContent, OpmlOperation Operation)> ExportToOpmlAsync(string fileName);

        /// <summary>
        /// Get an OPML operation by ID
        /// </summary>
        /// <param name="operationId">ID of the operation</param>
        /// <returns>The OPML operation</returns>
        Task<OpmlOperation?> GetOperationAsync(int operationId);

        /// <summary>
        /// Get recent OPML operations
        /// </summary>
        /// <param name="count">Maximum number of operations to return</param>
        /// <returns>List of recent OPML operations</returns>
        Task<IEnumerable<OpmlOperation>> GetRecentOperationsAsync(int count = 10);

        /// <summary>
        /// Cancel an in-progress OPML operation
        /// </summary>
        /// <param name="operationId">ID of the operation to cancel</param>
        /// <returns>The canceled operation</returns>
        Task<OpmlOperation?> CancelOperationAsync(int operationId);
    }
}
