using AppCore.Models;
using AppCore.Models.Bookmarks;
using AppCore.Models.Tags;
using AppCore.Repositories;
using AppCore.Services.Bookmarks;
using AppCore.UnitTests.Mocks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppCore.UnitTests.Services.Bookmarks
{
    [TestFixture]
    public class BookmarkServiceTests
    {
        private IRepository<Bookmark> _bookmarkRepository;
        private IRepository<Article> _articleRepository;
        private IRepository<Tag> _tagRepository;
        private IRepository<BookmarkTag> _bookmarkTagRepository;
        private IValidator<Bookmark> _bookmarkValidator;
        private BookmarkService _bookmarkService;

        [SetUp]
        public void Setup()
        {
            // Initialize repositories
            _bookmarkRepository = new MockRepository<Bookmark>();
            _articleRepository = new MockRepository<Article>();
            _tagRepository = new MockRepository<Tag>();
            _bookmarkTagRepository = new MockRepository<BookmarkTag>();
            
            // Create mock validator
            _bookmarkValidator = Substitute.For<IValidator<Bookmark>>();
            
            // Default setup for validator - validation passes by default
            _bookmarkValidator.Validate(Arg.Any<Bookmark>())
                .Returns(new ValidationResult());
            
            // Create service
            _bookmarkService = new BookmarkService(
                _bookmarkRepository, 
                _articleRepository, 
                _tagRepository, 
                _bookmarkTagRepository, 
                _bookmarkValidator);
        }

        [Test]
        public async Task GetByArticleIdAsync_WithValidId_ReturnsBookmark()
        {
            // Arrange
            var article = new Article { Id = 1, Title = "Test Article" };
            await _articleRepository.AddAsync(article);

            var bookmark = new Bookmark { ArticleId = 1, Title = "Test Bookmark", Notes = "Test notes" };
            await _bookmarkRepository.AddAsync(bookmark);

            // Act
            var result = await _bookmarkService.GetByArticleIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.ArticleId.Should().Be(1);
            result.Title.Should().Be("Test Bookmark");
        }

        [Test]
        public void GetByArticleIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Arrange
            // No setup needed

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => 
                _bookmarkService.GetByArticleIdAsync(-1));
            
            exception.Message.Should().Contain("Article ID must be greater than zero");
        }

        [Test]
        public async Task GetByArticleIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            // No bookmarks with articleId 999

            // Act
            var result = await _bookmarkService.GetByArticleIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task BookmarkArticleAsync_WithValidArticle_CreatesBookmark()
        {
            // Arrange
            var article = new Article { Id = 1, Title = "Test Article" };
            await _articleRepository.AddAsync(article);

            // Act
            var result = await _bookmarkService.BookmarkArticleAsync(1, "Some notes");

            // Assert
            result.Should().NotBeNull();
            result!.ArticleId.Should().Be(1);
            result.Notes.Should().Be("Some notes");

            // Verify bookmark was added to repository
            var bookmarks = await _bookmarkRepository.GetAllAsync();
            bookmarks.Should().ContainSingle();
            bookmarks.First().ArticleId.Should().Be(1);
        }

        [Test]
        public void BookmarkArticleAsync_WithNonExistentArticle_ThrowsKeyNotFoundException()
        {
            // Arrange
            // No article with ID 1

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _bookmarkService.BookmarkArticleAsync(1));
            
            exception.Message.Should().Contain("Article with ID 1 not found");

            // Verify no bookmark was added
            var bookmarks = _bookmarkRepository.GetAllAsync().Result;
            bookmarks.Should().BeEmpty();
        }

        [Test]
        public async Task BookmarkArticleAsync_WithAlreadyBookmarkedArticle_ReturnsExistingBookmark()
        {
            // Arrange
            var article = new Article { Id = 1, Title = "Test Article" };
            await _articleRepository.AddAsync(article);
            
            var bookmark = new Bookmark { ArticleId = 1, Title = "Test Article", Notes = "Existing notes" };
            await _bookmarkRepository.AddAsync(bookmark);

            // Act
            var result = await _bookmarkService.BookmarkArticleAsync(1, "New notes");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(bookmark.Id);
            result.Notes.Should().Be("Existing notes"); // Notes should not be updated

            // Verify no new bookmark was added
            var bookmarks = await _bookmarkRepository.GetAllAsync();
            bookmarks.Should().ContainSingle();
        }

        [Test]
        public async Task AddTagsToBookmarkAsync_WithValidIds_AddsTags()
        {
            // Arrange
            var bookmark = new Bookmark { Id = 1, Title = "Test Bookmark" };
            await _bookmarkRepository.AddAsync(bookmark);

            var tag1 = new Tag { Id = 1, Name = "Tag1" };
            var tag2 = new Tag { Id = 2, Name = "Tag2" };
            await _tagRepository.AddAsync(tag1);
            await _tagRepository.AddAsync(tag2);

            // Act
            var result = await _bookmarkService.AddTagsToBookmarkAsync(1, new[] { 1, 2 });

            // Assert
            result.Should().NotBeNull();
            
            // Verify bookmark-tag relations were created
            var bookmarkTags = await _bookmarkTagRepository.GetAllAsync();
            bookmarkTags.Should().HaveCount(2);
            bookmarkTags.Should().Contain(bt => bt.BookmarkId == 1 && bt.TagId == 1);
            bookmarkTags.Should().Contain(bt => bt.BookmarkId == 1 && bt.TagId == 2);
        }

        [Test]
        public async Task AddTagsToBookmarkAsync_WithNonExistentBookmark_ReturnsNull()
        {
            // Arrange
            var tag1 = new Tag { Id = 1, Name = "Tag1" };
            await _tagRepository.AddAsync(tag1);

            // Act
            var result = await _bookmarkService.AddTagsToBookmarkAsync(999, new[] { 1 });

            // Assert
            result.Should().BeNull();
            
            // Verify no bookmark-tag relations were created
            var bookmarkTags = await _bookmarkTagRepository.GetAllAsync();
            bookmarkTags.Should().BeEmpty();
        }

        [Test]
        public async Task RemoveTagsFromBookmarkAsync_WithValidIds_RemovesTags()
        {
            // Arrange
            var bookmark = new Bookmark { Id = 1, Title = "Test Bookmark" };
            await _bookmarkRepository.AddAsync(bookmark);

            var tag1 = new Tag { Id = 1, Name = "Tag1" };
            var tag2 = new Tag { Id = 2, Name = "Tag2" };
            await _tagRepository.AddAsync(tag1);
            await _tagRepository.AddAsync(tag2);

            var bookmarkTag1 = new BookmarkTag { BookmarkId = 1, TagId = 1 };
            var bookmarkTag2 = new BookmarkTag { BookmarkId = 1, TagId = 2 };
            await _bookmarkTagRepository.AddAsync(bookmarkTag1);
            await _bookmarkTagRepository.AddAsync(bookmarkTag2);

            // Act
            var result = await _bookmarkService.RemoveTagsFromBookmarkAsync(1, new[] { 1 });

            // Assert
            result.Should().NotBeNull();
            
            // Verify bookmark-tag relation was removed
            var bookmarkTags = await _bookmarkTagRepository.GetAllAsync();
            bookmarkTags.Should().ContainSingle();
            bookmarkTags.Should().Contain(bt => bt.BookmarkId == 1 && bt.TagId == 2);
        }

        [Test]
        public async Task GetBookmarksByTagAsync_WithValidTag_ReturnsBookmarks()
        {
            // Arrange
            var bookmark1 = new Bookmark { Id = 1, Title = "Bookmark 1" };
            var bookmark2 = new Bookmark { Id = 2, Title = "Bookmark 2" };
            await _bookmarkRepository.AddAsync(bookmark1);
            await _bookmarkRepository.AddAsync(bookmark2);

            var tag = new Tag { Id = 1, Name = "Test Tag" };
            await _tagRepository.AddAsync(tag);

            var bookmarkTag1 = new BookmarkTag { BookmarkId = 1, TagId = 1 };
            var bookmarkTag2 = new BookmarkTag { BookmarkId = 2, TagId = 1 };
            await _bookmarkTagRepository.AddAsync(bookmarkTag1);
            await _bookmarkTagRepository.AddAsync(bookmarkTag2);

            // Act
            var result = await _bookmarkService.GetBookmarksByTagAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(b => b.Id == 1);
            result.Should().Contain(b => b.Id == 2);
        }

        [Test]
        public void GetBookmarksByTagAsync_WithNonExistentTag_ThrowsKeyNotFoundException()
        {
            // Arrange
            var bookmark = new Bookmark { Id = 1, Title = "Test Bookmark" };
            _bookmarkRepository.AddAsync(bookmark).Wait();

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _bookmarkService.GetBookmarksByTagAsync(999));
            
            exception.Message.Should().Contain("Tag with ID 999 not found");
        }
    }
}
