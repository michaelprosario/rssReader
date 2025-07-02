using AppCore.Models.Feeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppInfra.Data
{
    /// <summary>
    /// Database initialization helper
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Initialize the database with sample data if needed
        /// </summary>
        /// <param name="context">The database context</param>
        public static async Task Initialize(AppDbContext context)
        {
            // Check if we already have feeds
            if (await context.Set<Feed>().AnyAsync())
            {
                // DB already has data
                return;
            }

            // Add some sample feeds only if no feeds exist
            var sampleFeeds = new[]
            {
                new Feed
                {
                    Title = "The Verge",
                    Description = "The Verge covers the intersection of technology, science, art, and culture.",
                    FeedUrl = "https://www.theverge.com/rss/index.xml",
                    WebsiteUrl = "https://www.theverge.com",
                    FeedType = FeedType.RSS,
                    IsActive = true,
                },
                new Feed
                {
                    Title = "Hacker News",
                    Description = "Hacker News is a social news website focusing on computer science and entrepreneurship.",
                    FeedUrl = "https://news.ycombinator.com/rss",
                    WebsiteUrl = "https://news.ycombinator.com",
                    FeedType = FeedType.RSS,
                    IsActive = true,
                }
            };

            await context.AddRangeAsync(sampleFeeds);
            await context.SaveChangesAsync();
        }
    }
}