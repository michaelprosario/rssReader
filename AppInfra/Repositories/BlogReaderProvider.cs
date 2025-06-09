namespace AppInfra.Repositories
{
    using AppCore.Models;
    using AppCore.Models.Feeds;
    using AppCore.Repositories;
    using AppCore.Services.Articles;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;
    using Sagara.FeedReader;

    /// <summary>
    /// Provides access to blog reader data.
    /// </summary>
    public class BlogReaderProvider : IBlogReaderProvider
    {
        private readonly IRepository<AppCore.Models.Feeds.Feed> _repository;
        private readonly FeedReader _feedReader;

        public BlogReaderProvider(
            IRepository<AppCore.Models.Feeds.Feed> repository,
            FeedReader feedReader
        )
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _feedReader = feedReader ?? throw new ArgumentNullException(nameof(feedReader));
        }

        public async Task<List<Article>> GetArticlesFromFeed(Guid feedId, int maxCount)
        {
            AppCore.Models.Feeds.Feed? feed = await _repository.GetByIdAsync(feedId);
            if (feed == null)
            {
                throw new ApplicationException($"Feed with ID {feedId} not found.");
            }

            string feedUrl = feed.FeedUrl;

            var feedResponse = await _feedReader.ReadFromUrlAsync(feedUrl);
            var articles = new List<Article>();

            //readRssResponse.Title = feed.Title;
            //readRssResponse.Description = feed.Description;
            //readRssResponse.ImageLink = feed.ImageUrl;

            foreach (var item in feedResponse.Items)
            {
                var contentItem = new Article
                {
                    Title = item.Title,
                    Summary = item.Description,
                    Author = "",
                    PublishedAt = item.PublishingDate != null ? (DateTime)item.PublishingDate : DateTime.Now,
                    Url = item.Link,
                };

                articles.Add(contentItem);
            }

            return articles
                .OrderByDescending(a => a.PublishedAt)
                .Take(maxCount)
                .ToList();

        }
    }
}
  