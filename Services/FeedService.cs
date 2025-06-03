using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.ServiceModel.Syndication;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Net.Http;
using rssReader.Models;
using System.Text.RegularExpressions;

namespace rssReader.Services
{
    /// <summary>
    /// Interface for feed management operations.
    /// </summary>
    public interface IFeedService
    {
        /// <summary>
        /// Gets all feeds.
        /// </summary>
        Task<List<Feed>> GetAllFeedsAsync();
        
        /// <summary>
        /// Gets a feed by ID.
        /// </summary>
        Task<Feed> GetFeedByIdAsync(string id);
        
        /// <summary>
        /// Adds a new feed.
        /// </summary>
        Task<Feed> AddFeedAsync(Feed feed);
        
        /// <summary>
        /// Adds a feed by URL, discovering feed information.
        /// </summary>
        Task<Feed> AddFeedByUrlAsync(string url);
        
        /// <summary>
        /// Updates an existing feed.
        /// </summary>
        Task<Feed> UpdateFeedAsync(Feed feed);
        
        /// <summary>
        /// Removes a feed.
        /// </summary>
        Task<bool> RemoveFeedAsync(string id);        /// <summary>
        /// Refreshes all feeds or a specific feed.
        /// </summary>
        Task<int> RefreshFeedsAsync(string feedId = null);
        
        /// <summary>
        /// Imports feeds from an OPML file.
        /// </summary>
        Task<int> ImportOpmlAsync(Stream opmlStream);
        
        /// <summary>
        /// Exports feeds to an OPML file.
        /// </summary>
        Task<string> ExportOpmlAsync();
        
        /// <summary>
        /// Discovers feeds from a website URL.
        /// </summary>
        Task<List<Feed>> DiscoverFeedsAsync(string websiteUrl);
    }

    /// <summary>
    /// Implementation of the feed service.
    /// </summary>
    public class FeedService : IFeedService
    {
        private readonly IDataStorageService _dataStorage;
        private readonly IArticleService _articleService;
        private readonly HttpClient _httpClient;

        private const string FEEDS_FILE = "feeds.json";

        /// <summary>
        /// Constructor for FeedService.
        /// </summary>
        public FeedService(IDataStorageService dataStorage, IArticleService articleService, HttpClient httpClient)
        {
            _dataStorage = dataStorage;
            _articleService = articleService;
            _httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<List<Feed>> GetAllFeedsAsync()
        {
            var feeds = await _dataStorage.LoadDataAsync<List<Feed>>(FEEDS_FILE) ?? new List<Feed>();
            
            // Get unread counts for each feed
            foreach (var feed in feeds)
            {
                feed.UnreadCount = await _articleService.GetUnreadCountAsync(feed.Id);
            }
            
            return feeds;
        }

        /// <inheritdoc/>
        public async Task<Feed> GetFeedByIdAsync(string id)
        {
            var feeds = await GetAllFeedsAsync();
            var feed = feeds.FirstOrDefault(f => f.Id == id);
            
            if (feed != null)
            {
                feed.UnreadCount = await _articleService.GetUnreadCountAsync(feed.Id);
            }
            
            return feed;
        }

        /// <inheritdoc/>
        public async Task<Feed> AddFeedAsync(Feed feed)
        {
            var feeds = await GetAllFeedsAsync();
            
            // Check for duplicate URL
            if (feeds.Any(f => f.Url.Equals(feed.Url, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A feed with this URL already exists.");
            }
            
            // Set defaults if not provided
            if (string.IsNullOrWhiteSpace(feed.Id))
            {
                feed.Id = Guid.NewGuid().ToString();
            }
            
            feed.DateAdded = DateTime.Now;
            feed.LastUpdated = DateTime.Now;
            
            feeds.Add(feed);
            await _dataStorage.SaveDataAsync(FEEDS_FILE, feeds);
            
            // Initial fetch of articles
            await RefreshFeedsAsync(feed.Id);
            
            return feed;
        }

        /// <inheritdoc/>
        public async Task<Feed> AddFeedByUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be empty", nameof(url));
            }

            // Check if URL is a website rather than a direct feed
            if (!url.Contains("rss", StringComparison.OrdinalIgnoreCase) && 
                !url.Contains("feed", StringComparison.OrdinalIgnoreCase) &&
                !url.Contains("atom", StringComparison.OrdinalIgnoreCase) &&
                !url.EndsWith("xml", StringComparison.OrdinalIgnoreCase))
            {
                // Try to discover feeds
                var discoveredFeeds = await DiscoverFeedsAsync(url);
                if (discoveredFeeds.Any())
                {
                    // Use the first discovered feed
                    return await AddFeedAsync(discoveredFeeds.First());
                }
            }

            // Proceed with direct feed URL
            try
            {
                var feed = await ParseFeedFromUrlAsync(url);
                return await AddFeedAsync(feed);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse feed: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Feed> UpdateFeedAsync(Feed feed)
        {
            var feeds = await GetAllFeedsAsync();
            var existingFeed = feeds.FirstOrDefault(f => f.Id == feed.Id);
            
            if (existingFeed == null)
            {
                throw new InvalidOperationException($"Feed with ID {feed.Id} not found.");
            }
            
            // Check for duplicate URL (if changed)
            if (!existingFeed.Url.Equals(feed.Url, StringComparison.OrdinalIgnoreCase) && 
                feeds.Any(f => f.Id != feed.Id && f.Url.Equals(feed.Url, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Another feed with this URL already exists.");
            }
            
            // Update properties
            existingFeed.Title = feed.Title;
            existingFeed.Description = feed.Description;
            existingFeed.Url = feed.Url;
            existingFeed.WebsiteUrl = feed.WebsiteUrl;
            existingFeed.RefreshIntervalMinutes = feed.RefreshIntervalMinutes;
            existingFeed.Format = feed.Format;
            existingFeed.LogoUrl = feed.LogoUrl;
            existingFeed.IsEnabled = feed.IsEnabled;
            
            await _dataStorage.SaveDataAsync(FEEDS_FILE, feeds);
            return existingFeed;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveFeedAsync(string id)
        {
            var feeds = await GetAllFeedsAsync();
            var feed = feeds.FirstOrDefault(f => f.Id == id);
            
            if (feed == null)
            {
                return false;
            }
            
            feeds.Remove(feed);
            await _dataStorage.SaveDataAsync(FEEDS_FILE, feeds);
            
            // Delete articles associated with this feed
            await _articleService.DeleteArticlesByFeedIdAsync(id);
            
            return true;
        }

        /// <inheritdoc/>
        public async Task<int> RefreshFeedsAsync(string feedId = null)
        {
            var feeds = await GetAllFeedsAsync();
            int newArticlesCount = 0;
            
            // Filter feeds if a specific ID is provided
            if (!string.IsNullOrWhiteSpace(feedId))
            {
                feeds = feeds.Where(f => f.Id == feedId).ToList();
            }
            
            // Only process enabled feeds
            feeds = feeds.Where(f => f.IsEnabled).ToList();
            
            foreach (var feed in feeds)
            {
                try
                {
                    var articles = await FetchArticlesFromFeedAsync(feed);
                    int added = await _articleService.AddArticlesAsync(articles, feed.Id);
                    newArticlesCount += added;
                    
                    // Update last updated timestamp
                    feed.LastUpdated = DateTime.Now;
                }
                catch (Exception)
                {
                    // Log error but continue with other feeds
                    continue;
                }
            }
            
            // Save updated feeds
            await _dataStorage.SaveDataAsync(FEEDS_FILE, feeds);
            
            return newArticlesCount;
        }

        /// <inheritdoc/>
        public async Task<int> ImportOpmlAsync(Stream opmlStream)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(opmlStream);
                
                // Get all outline elements with an xmlUrl attribute
                var feedNodes = xmlDoc.SelectNodes("//outline[@xmlUrl]");
                if (feedNodes == null)
                {
                    return 0;
                }
                
                int importCount = 0;
                foreach (XmlNode node in feedNodes)
                {
                    var xmlUrl = node.Attributes["xmlUrl"]?.Value;
                    var title = node.Attributes["title"]?.Value ?? node.Attributes["text"]?.Value;
                    var htmlUrl = node.Attributes["htmlUrl"]?.Value;
                    
                    if (!string.IsNullOrWhiteSpace(xmlUrl))
                    {
                        var feed = new Feed
                        {
                            Title = title ?? "Imported Feed",
                            Url = xmlUrl,
                            WebsiteUrl = htmlUrl,
                            Description = node.Attributes["description"]?.Value
                        };
                        
                        try
                        {
                            await AddFeedAsync(feed);
                            importCount++;
                        }
                        catch
                        {
                            // Skip feeds that can't be added (e.g., duplicates)
                            continue;
                        }
                    }
                }
                
                return importCount;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to import OPML: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> ExportOpmlAsync()
        {
            var feeds = await GetAllFeedsAsync();
            
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  "
            };
            
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("opml");
                xmlWriter.WriteAttributeString("version", "1.0");
                
                xmlWriter.WriteStartElement("head");
                xmlWriter.WriteElementString("title", "RSS Reader Feed Export");
                xmlWriter.WriteElementString("dateCreated", DateTime.Now.ToString("r"));
                xmlWriter.WriteEndElement(); // head
                
                xmlWriter.WriteStartElement("body");
                
                foreach (var feed in feeds)
                {
                    xmlWriter.WriteStartElement("outline");
                    xmlWriter.WriteAttributeString("text", feed.Title);
                    xmlWriter.WriteAttributeString("title", feed.Title);
                    xmlWriter.WriteAttributeString("type", "rss");
                    xmlWriter.WriteAttributeString("xmlUrl", feed.Url);
                    
                    if (!string.IsNullOrWhiteSpace(feed.WebsiteUrl))
                    {
                        xmlWriter.WriteAttributeString("htmlUrl", feed.WebsiteUrl);
                    }
                    
                    if (!string.IsNullOrWhiteSpace(feed.Description))
                    {
                        xmlWriter.WriteAttributeString("description", feed.Description);
                    }
                    
                    xmlWriter.WriteEndElement(); // outline
                }
                
                xmlWriter.WriteEndElement(); // body
                xmlWriter.WriteEndElement(); // opml
                xmlWriter.WriteEndDocument();
                
                return stringWriter.ToString();
            }
        }

        /// <inheritdoc/>
        public async Task<List<Feed>> DiscoverFeedsAsync(string websiteUrl)
        {
            if (string.IsNullOrWhiteSpace(websiteUrl))
            {
                throw new ArgumentException("Website URL cannot be empty", nameof(websiteUrl));
            }
            
            // Ensure URL has scheme
            if (!websiteUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                !websiteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                websiteUrl = "https://" + websiteUrl;
            }
            
            try
            {
                var response = await _httpClient.GetAsync(websiteUrl);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(content);
                
                var possibleFeedLinks = new List<Feed>();
                
                // Look for link tags with feed references
                var linkNodes = doc.DocumentNode.SelectNodes("//link[@rel='alternate'][@type]");
                if (linkNodes != null)
                {
                    foreach (var node in linkNodes)
                    {
                        var type = node.GetAttributeValue("type", "").ToLowerInvariant();
                        var href = node.GetAttributeValue("href", "");
                        var title = node.GetAttributeValue("title", "");
                        
                        if (!string.IsNullOrWhiteSpace(href) && 
                            (type.Contains("rss") || type.Contains("atom") || type.Contains("xml") || type.Contains("json")))
                        {
                            // Handle relative URLs
                            Uri absoluteUri;
                            if (Uri.TryCreate(new Uri(websiteUrl), href, out absoluteUri))
                            {
                                href = absoluteUri.ToString();
                            }
                            
                            FeedFormat format = FeedFormat.RSS;
                            if (type.Contains("atom"))
                            {
                                format = FeedFormat.Atom;
                            }
                            else if (type.Contains("json"))
                            {
                                format = FeedFormat.JSON;
                            }
                            
                            possibleFeedLinks.Add(new Feed
                            {
                                Title = !string.IsNullOrWhiteSpace(title) ? title : "Discovered Feed",
                                Url = href,
                                WebsiteUrl = websiteUrl,
                                Format = format
                            });
                        }
                    }
                }
                
                // Look for a's with feed-like URLs
                var anchorNodes = doc.DocumentNode.SelectNodes("//a");
                if (anchorNodes != null)
                {
                    foreach (var node in anchorNodes)
                    {
                        var href = node.GetAttributeValue("href", "").ToLowerInvariant();
                        var text = node.InnerText;
                        
                        if (!string.IsNullOrWhiteSpace(href) && 
                            (href.Contains("rss") || href.Contains("feed") || href.Contains("atom") || href.EndsWith(".xml")))
                        {
                            // Handle relative URLs
                            Uri absoluteUri;
                            if (Uri.TryCreate(new Uri(websiteUrl), href, out absoluteUri))
                            {
                                href = absoluteUri.ToString();
                            }
                            
                            // Skip if we already found this URL
                            if (possibleFeedLinks.Any(f => f.Url.Equals(href, StringComparison.OrdinalIgnoreCase)))
                            {
                                continue;
                            }
                            
                            possibleFeedLinks.Add(new Feed
                            {
                                Title = !string.IsNullOrWhiteSpace(text) ? text : "Discovered Feed",
                                Url = href,
                                WebsiteUrl = websiteUrl,
                                Format = FeedFormat.RSS
                            });
                        }
                    }
                }
                
                // Try validating each feed
                var validatedFeeds = new List<Feed>();
                foreach (var feedCandidate in possibleFeedLinks)
                {
                    try
                    {
                        var feed = await ParseFeedFromUrlAsync(feedCandidate.Url);
                        feed.WebsiteUrl = websiteUrl;
                        validatedFeeds.Add(feed);
                    }
                    catch
                    {
                        // Skip invalid feeds
                        continue;
                    }
                }
                
                return validatedFeeds;
                
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to discover feeds: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parse a feed from a URL and return a Feed object.
        /// </summary>
        private async Task<Feed> ParseFeedFromUrlAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                SyndicationFeed syndicationFeed;
                
                using (var stringReader = new StringReader(content))
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    syndicationFeed = SyndicationFeed.Load(xmlReader);
                }
                
                var feed = new Feed
                {
                    Title = syndicationFeed.Title?.Text ?? "Untitled Feed",
                    Description = syndicationFeed.Description?.Text ?? "",
                    Url = url,
                    WebsiteUrl = syndicationFeed.Links.FirstOrDefault(l => l.RelationshipType == "alternate")?.Uri?.ToString(),
                    Format = FeedFormat.RSS // Assuming RSS by default
                };
                
                // Try to get logo/icon
                if (syndicationFeed.ImageUrl != null)
                {
                    feed.LogoUrl = syndicationFeed.ImageUrl.ToString();
                }
                
                return feed;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse feed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Fetches articles from a feed.
        /// </summary>
        private async Task<List<Article>> FetchArticlesFromFeedAsync(Feed feed)
        {
            try
            {
                var response = await _httpClient.GetAsync(feed.Url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                SyndicationFeed syndicationFeed;
                using (var stringReader = new StringReader(content))
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    syndicationFeed = SyndicationFeed.Load(xmlReader);
                }
                
                var articles = new List<Article>();
                
                foreach (var item in syndicationFeed.Items)
                {
                    var article = new Article
                    {
                        FeedId = feed.Id,
                        Title = item.Title?.Text ?? "Untitled",
                        Summary = item.Summary?.Text ?? "",
                        Url = item.Links.FirstOrDefault()?.Uri.ToString() ?? "",
                        PublishDate = item.PublishDate.DateTime != DateTime.MinValue ? item.PublishDate.DateTime : DateTime.Now,
                        OriginalId = item.Id ?? Guid.NewGuid().ToString(),
                        FeedTitle = feed.Title
                    };
                    
                    // Try to get content
                    var contentElement = item.ElementExtensions.FirstOrDefault(e => e.OuterName == "content");
                    if (contentElement != null)
                    {
                        article.Content = contentElement.GetObject<XmlElement>().InnerText;
                        article.IsFullContentFetched = true;
                    }
                    
                    // Get authors
                    if (item.Authors != null && item.Authors.Count > 0)
                    {
                        article.Authors = item.Authors.Select(a => a.Name).ToList();
                    }
                    
                    // Get categories
                    if (item.Categories != null && item.Categories.Count > 0)
                    {
                        article.Categories = item.Categories.Select(c => c.Name).ToList();
                    }
                    
                    // Try to extract an image
                    if (!string.IsNullOrWhiteSpace(article.Content))
                    {
                        var imgMatch = Regex.Match(article.Content, @"<img.+?src=[""'](.+?)[""'].*?>");
                        if (imgMatch.Success)
                        {
                            article.ImageUrl = imgMatch.Groups[1].Value;
                        }
                    }
                    
                    articles.Add(article);
                }
                
                return articles;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to fetch articles: {ex.Message}", ex);
            }
        }
    }
}
