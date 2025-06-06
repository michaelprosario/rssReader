using AppCore.Models;
using AppCore.Models.Feeds;
using AppCore.Repositories;
using AppCore.Services.Articles;
using AppCore.UnitTests.Mocks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppCore.UnitTests.Services.Articles
{
    [TestFixture]
    public class ArticleServiceTests
    {
        private IRepository<Article> _articleRepository;
        private IRepository<Feed> _feedRepository;
        private IValidator<Article> _articleValidator;
        private ArticleService _articleService;

        [SetUp]
        public void Setup()
        {
            // Initialize repositories
            _articleRepository = new MockRepository<Article>();
            _feedRepository = new MockRepository<Feed>();
            
            // Create mock validator
            _articleValidator = Substitute.For<IValidator<Article>>();
            
            // Default setup for validator - validation passes by default
            _articleValidator.Validate(Arg.Any<Article>())
                .Returns(new ValidationResult());
            
            // Create service
            _articleService = new ArticleService(_articleRepository, _feedRepository, _articleValidator);
        }

        [Test]
        public async Task AddAsync_ValidArticle_AddsArticle()
        {
            // Arrange
            var feed = new Feed { Title = "Test Feed", FeedUrl = "https://example.com/feed" };
            feed = await _feedRepository.AddAsync(feed);
            
            var article = new Article
            {
                Title = "Test Article",
                Url = "https://example.com/article1",
                FeedId = feed.Id,
                PublishedAt = DateTime.UtcNow,
                UniqueId = "article-1"
            };

            // Act
            var result = await _articleService.AddAsync(article);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Title.Should().Be("Test Article");
            
            // Verify it was added to the repository
            var savedArticle = await _articleRepository.GetByIdAsync(result.Id);
            savedArticle.Should().NotBeNull();
            savedArticle!.Title.Should().Be("Test Article");
        }

        [Test]
        public void AddAsync_InvalidArticle_ThrowsValidationException()
        {
            // Arrange
            var article = new Article
            {
                // Missing required fields
            };
            
            // Configure validator to fail
            var validationFailure = new ValidationFailure("Title", "Title is required");
            var validationResult = new ValidationResult(new[] { validationFailure });
            _articleValidator.Validate(Arg.Any<Article>()).Returns(validationResult);

            // Act & Assert
            Func<Task> act = async () => await _articleService.AddAsync(article);
            act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task GetArticlesByFeedAsync_ExistingFeed_ReturnsArticles()
        {
            // Arrange
            var feed = new Feed { Title = "Test Feed", FeedUrl = "https://example.com/feed" };
            feed = await _feedRepository.AddAsync(feed);
            
            var article1 = new Article 
            { 
                Title = "Article 1", 
                Url = "https://example.com/article1", 
                FeedId = feed.Id,
                UniqueId = "article-1",
                PublishedAt = DateTime.UtcNow 
            };
            
            var article2 = new Article 
            { 
                Title = "Article 2", 
                Url = "https://example.com/article2", 
                FeedId = feed.Id,
                UniqueId = "article-2",
                PublishedAt = DateTime.UtcNow
            };
            
            await _articleRepository.AddAsync(article1);
            await _articleRepository.AddAsync(article2);

            // Act
            var results = await _articleService.GetArticlesByFeedAsync(feed.Id);

            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(2);
            results.Should().Contain(a => a.Title == "Article 1");
            results.Should().Contain(a => a.Title == "Article 2");
        }

        [Test]
        public async Task GetArticlesByFeedAsync_NonExistentFeed_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            Func<Task> act = async () => await _articleService.GetArticlesByFeedAsync(999);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Test]
        public async Task GetUnreadArticlesAsync_WithUnreadArticles_ReturnsOnlyUnread()
        {
            // Arrange
            var feed = new Feed { Title = "Test Feed", FeedUrl = "https://example.com/feed" };
            feed = await _feedRepository.AddAsync(feed);
            
            var article1 = new Article 
            { 
                Title = "Article 1", 
                Url = "https://example.com/article1", 
                FeedId = feed.Id,
                IsRead = false,
                UniqueId = "article-1",
                PublishedAt = DateTime.UtcNow 
            };
            
            var article2 = new Article 
            { 
                Title = "Article 2", 
                Url = "https://example.com/article2", 
                FeedId = feed.Id,
                IsRead = true,
                UniqueId = "article-2",
                PublishedAt = DateTime.UtcNow
            };
            
            var article3 = new Article 
            { 
                Title = "Article 3", 
                Url = "https://example.com/article3", 
                FeedId = feed.Id,
                IsRead = false,
                UniqueId = "article-3",
                PublishedAt = DateTime.UtcNow
            };
            
            await _articleRepository.AddAsync(article1);
            await _articleRepository.AddAsync(article2);
            await _articleRepository.AddAsync(article3);

            // Act
            var results = await _articleService.GetUnreadArticlesAsync();

            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(2);
            results.Should().Contain(a => a.Title == "Article 1");
            results.Should().Contain(a => a.Title == "Article 3");
            results.Should().NotContain(a => a.Title == "Article 2");
        }

        [Test]
        public async Task MarkAsReadAsync_UnreadArticle_MarksAsReadAndUpdatesFeed()
        {
            // Arrange
            var feed = new Feed { Title = "Test Feed", FeedUrl = "https://example.com/feed", UnreadCount = 1 };
            feed = await _feedRepository.AddAsync(feed);
            
            var article = new Article 
            { 
                Title = "Article 1", 
                Url = "https://example.com/article1", 
                FeedId = feed.Id,
                IsRead = false,
                UniqueId = "article-1",
                PublishedAt = DateTime.UtcNow 
            };
            
            article = await _articleRepository.AddAsync(article);

            // Act
            var result = await _articleService.MarkAsReadAsync(article.Id);

            // Assert
            result.Should().NotBeNull();
            result!.IsRead.Should().BeTrue();
            
            // Verify feed unread count is updated
            var updatedFeed = await _feedRepository.GetByIdAsync(feed.Id);
            updatedFeed!.UnreadCount.Should().Be(0);
        }

        [Test]
        public async Task MarkAsUnreadAsync_ReadArticle_MarksAsUnreadAndUpdatesFeed()
        {
            // Arrange
            var feed = new Feed { Title = "Test Feed", FeedUrl = "https://example.com/feed", UnreadCount = 0 };
            feed = await _feedRepository.AddAsync(feed);
            
            var article = new Article 
            { 
                Title = "Article 1", 
                Url = "https://example.com/article1", 
                FeedId = feed.Id,
                IsRead = true,
                UniqueId = "article-1",
                PublishedAt = DateTime.UtcNow 
            };
            
            article = await _articleRepository.AddAsync(article);

            // Act
            var result = await _articleService.MarkAsUnreadAsync(article.Id);

            // Assert
            result.Should().NotBeNull();
            result!.IsRead.Should().BeFalse();
            
            // Verify feed unread count is updated
            var updatedFeed = await _feedRepository.GetByIdAsync(feed.Id);
            updatedFeed!.UnreadCount.Should().Be(1);
        }

        [Test]
        public async Task FetchFullContentAsync_ValidArticle_UpdatesFullContentStatus()
        {
            // Arrange
            var feed = new Feed { Title = "Test Feed", FeedUrl = "https://example.com/feed" };
            feed = await _feedRepository.AddAsync(feed);
            
            var article = new Article 
            { 
                Title = "Article 1", 
                Url = "https://example.com/article1", 
                FeedId = feed.Id,
                HasFullContent = false,
                UniqueId = "article-1",
                PublishedAt = DateTime.UtcNow 
            };
            
            article = await _articleRepository.AddAsync(article);

            // Act
            var result = await _articleService.FetchFullContentAsync(article.Id);

            // Assert
            result.Should().NotBeNull();
            result!.HasFullContent.Should().BeTrue();
            result.FullContentFetchedAt.Should().NotBeNull();
        }

        [Test]
        public async Task SearchArticlesAsync_WithMatchingContent_ReturnsMatchingArticles()
        {
            // Arrange
            var feed = new Feed { Title = "Test Feed", FeedUrl = "https://example.com/feed" };
            feed = await _feedRepository.AddAsync(feed);
            
            var article1 = new Article 
            { 
                Title = "Article about Testing", 
                Summary = "This is an article about unit testing",
                Url = "https://example.com/article1", 
                FeedId = feed.Id,
                UniqueId = "article-1",
                PublishedAt = DateTime.UtcNow 
            };
            
            var article2 = new Article 
            { 
                Title = "Article about Development", 
                Summary = "This is an article about software development",
                Url = "https://example.com/article2", 
                FeedId = feed.Id,
                UniqueId = "article-2",
                PublishedAt = DateTime.UtcNow
            };
            
            await _articleRepository.AddAsync(article1);
            await _articleRepository.AddAsync(article2);

            // Act
            var results = await _articleService.SearchArticlesAsync("testing");

            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(1);
            results.Should().Contain(a => a.Title == "Article about Testing");
        }

        [Test]
        public void SearchArticlesAsync_EmptySearchText_ThrowsArgumentException()
        {
            // Act & Assert
            Func<Task> act = async () => await _articleService.SearchArticlesAsync("");
            act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
