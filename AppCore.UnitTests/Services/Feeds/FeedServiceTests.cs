using AppCore.Models;
using AppCore.Models.Feeds;
using AppCore.Repositories;
using AppCore.Services.Feeds;
using AppCore.UnitTests.Mocks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppCore.UnitTests.Services.Feeds
{
    [TestFixture]
    public class FeedServiceTests
    {
        private IRepository<Feed> _feedRepository;
        private IRepository<Article> _articleRepository;
        private IValidator<Feed> _feedValidator;
        private FeedService _feedService;

        [SetUp]
        public void Setup()
        {
            // Initialize repositories
            _feedRepository = new MockRepository<Feed>();
            _articleRepository = new MockRepository<Article>();
            
            // Create mock validator
            _feedValidator = Substitute.For<IValidator<Feed>>();
            
            // Default setup for validator - validation passes by default
            _feedValidator.Validate(Arg.Any<Feed>())
                .Returns(new ValidationResult());
            
            // Create service
            _feedService = new FeedService(_feedRepository, _articleRepository, _feedValidator);
        }

        [Test]
        public async Task AddAsync_ValidFeed_AddsFeed()
        {
            // Arrange
            var feed = new Feed
            {
                Title = "Test Feed",
                FeedUrl = "https://example.com/feed",
                FeedType = FeedType.RSS
            };

            // Act
            var result = await _feedService.AddAsync(feed);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Title.Should().Be("Test Feed");
            result.FeedUrl.Should().Be("https://example.com/feed");
            
            // Verify it was added to the repository
            var savedFeed = await _feedRepository.GetByIdAsync(result.Id);
            savedFeed.Should().NotBeNull();
            savedFeed!.Title.Should().Be("Test Feed");
        }

        [Test]
        public void AddAsync_InvalidFeed_ThrowsValidationException()
        {
            // Arrange
            var feed = new Feed
            {
                // Missing required fields like Title and FeedUrl
            };
            
            // Configure validator to fail
            var validationFailure = new ValidationFailure("Title", "Title is required");
            var validationResult = new ValidationResult(new[] { validationFailure });
            _feedValidator.Validate(Arg.Any<Feed>()).Returns(validationResult);

            // Act & Assert
            Func<Task> act = async () => await _feedService.AddAsync(feed);
            act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task GetByUrlAsync_ExistingUrl_ReturnsFeed()
        {
            // Arrange
            var feedUrl = "https://example.com/existing-feed";
            var feed = new Feed
            {
                Title = "Existing Feed",
                FeedUrl = feedUrl,
                FeedType = FeedType.RSS
            };
            
            await _feedRepository.AddAsync(feed);

            // Act
            var result = await _feedService.GetByUrlAsync(feedUrl);

            // Assert
            result.Should().NotBeNull();
            result!.FeedUrl.Should().Be(feedUrl);
            result.Title.Should().Be("Existing Feed");
        }

        [Test]
        public async Task GetByUrlAsync_NonExistentUrl_ReturnsNull()
        {
            // Act
            var result = await _feedService.GetByUrlAsync("https://example.com/non-existent");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByUrlAsync_EmptyUrl_ThrowsArgumentException()
        {
            // Act & Assert
            Func<Task> act = async () => await _feedService.GetByUrlAsync("");
            act.Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task UpdateRefreshIntervalAsync_ValidFeedAndInterval_UpdatesInterval()
        {
            // Arrange
            var feed = new Feed
            {
                Title = "Test Feed",
                FeedUrl = "https://example.com/feed",
                FeedType = FeedType.RSS,
                RefreshIntervalMinutes = 60
            };
            
            feed = await _feedRepository.AddAsync(feed);
            var feedId = feed.Id;
            
            // Act
            var result = await _feedService.UpdateRefreshIntervalAsync(feedId, 30);

            // Assert
            result.Should().NotBeNull();
            result!.RefreshIntervalMinutes.Should().Be(30);
            
            // Verify updated in repository
            var updatedFeed = await _feedRepository.GetByIdAsync(feedId);
            updatedFeed.Should().NotBeNull();
            updatedFeed!.RefreshIntervalMinutes.Should().Be(30);
        }
        
        [Test]
        public async Task UpdateRefreshIntervalAsync_SetToNull_ResetsToGlobalDefault()
        {
            // Arrange
            var feed = new Feed
            {
                Title = "Test Feed",
                FeedUrl = "https://example.com/feed",
                FeedType = FeedType.RSS,
                RefreshIntervalMinutes = 60
            };
            
            feed = await _feedRepository.AddAsync(feed);
            var feedId = feed.Id;
            
            // Act
            var result = await _feedService.UpdateRefreshIntervalAsync(feedId, null);

            // Assert
            result.Should().NotBeNull();
            result!.RefreshIntervalMinutes.Should().BeNull();
        }
        
        [Test]
        public void UpdateRefreshIntervalAsync_NegativeInterval_ThrowsArgumentException()
        {
            // Arrange
            var feed = new Feed
            {
                Title = "Test Feed",
                FeedUrl = "https://example.com/feed",
                FeedType = FeedType.RSS
            };
            feed = _feedRepository.AddAsync(feed).Result;
            
            // Act & Assert
            Func<Task> act = async () => await _feedService.UpdateRefreshIntervalAsync(feed.Id, -1);
            act.Should().ThrowAsync<ArgumentException>();
        }
        
        [Test]
        public async Task MarkAllArticlesAsReadAsync_WithUnreadArticles_MarksAllAsRead()
        {
            // Arrange
            var feed = new Feed
            {
                Title = "Test Feed",
                FeedUrl = "https://example.com/feed",
                UnreadCount = 2
            };
            feed = await _feedRepository.AddAsync(feed);
            
            // Add some unread articles to this feed
            var article1 = new Article { FeedId = feed.Id, IsRead = false, Title = "Article 1" };
            var article2 = new Article { FeedId = feed.Id, IsRead = false, Title = "Article 2" };
            await _articleRepository.AddAsync(article1);
            await _articleRepository.AddAsync(article2);
            
            // Act
            var result = await _feedService.MarkAllArticlesAsReadAsync(feed.Id);
            
            // Assert
            result.Should().Be(2); // Two articles marked as read
            
            // Verify articles are marked as read
            var articles = await _articleRepository.FindAsync(a => a.FeedId == feed.Id);
            articles.All(a => a.IsRead).Should().BeTrue();
            
            // Verify feed's unread count is updated
            var updatedFeed = await _feedRepository.GetByIdAsync(feed.Id);
            updatedFeed!.UnreadCount.Should().Be(0);
        }
        
        [Test]
        public async Task DeleteAsync_ExistingFeed_DeletesFeed()
        {
            // Arrange
            var feed = new Feed
            {
                Title = "Test Feed",
                FeedUrl = "https://example.com/feed"
            };
            feed = await _feedRepository.AddAsync(feed);
            int feedId = feed.Id;
            
            // Act
            var result = await _feedService.DeleteAsync(feedId);
            
            // Assert
            result.Should().BeTrue();
            
            // Verify feed is deleted
            var deletedFeed = await _feedRepository.GetByIdAsync(feedId);
            deletedFeed.Should().BeNull();
        }
    }
}
