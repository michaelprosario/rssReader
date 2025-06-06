using AppCore.Models.Tags;
using AppCore.Repositories;
using AppCore.Services.Tags;
using AppCore.UnitTests.Mocks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AppCore.UnitTests.Services.Tags
{
    [TestFixture]
    public class TagServiceTests
    {
        private IRepository<Tag> _tagRepository;
        private IRepository<BookmarkTag> _bookmarkTagRepository;
        private IValidator<Tag> _tagValidator;
        private TagService _tagService;

        [SetUp]
        public void Setup()
        {
            // Initialize repositories
            _tagRepository = new MockRepository<Tag>();
            _bookmarkTagRepository = new MockRepository<BookmarkTag>();
            
            // Create mock validator
            _tagValidator = Substitute.For<IValidator<Tag>>();
            
            // Default setup for validator - validation passes by default
            _tagValidator.Validate(Arg.Any<Tag>())
                .Returns(new ValidationResult());
            
            // Create service
            _tagService = new TagService(_tagRepository, _bookmarkTagRepository, _tagValidator);
        }

        [Test]
        public async Task GetByNameAsync_WithValidName_ReturnsTag()
        {
            // Arrange
            var tag = new Tag { Name = "Technology", Color = "#FF5733" };
            await _tagRepository.AddAsync(tag);

            // Act
            var result = await _tagService.GetByNameAsync("Technology");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Technology");
            result.Color.Should().Be("#FF5733");
        }

        [Test]
        public async Task GetByNameAsync_WithNonExistentName_ReturnsNull()
        {
            // Arrange
            // No tag with name "NonExistent"
            
            // Act
            var result = await _tagService.GetByNameAsync("NonExistent");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetByNameAsync_WithNullOrEmptyName_ThrowsArgumentException()
        {
            // Arrange
            
            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => _tagService.GetByNameAsync(null!));
            Assert.ThrowsAsync<ArgumentException>(() => _tagService.GetByNameAsync(""));
            Assert.ThrowsAsync<ArgumentException>(() => _tagService.GetByNameAsync("  "));
        }

        [Test]
        public async Task GetByNameAsync_IsCaseInsensitive()
        {
            // Arrange
            var tag = new Tag { Name = "Technology", Color = "#FF5733" };
            await _tagRepository.AddAsync(tag);

            // Act
            var result = await _tagService.GetByNameAsync("technology");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Technology");
        }

        [Test]
        public async Task GetOrCreateTagAsync_WithExistingTag_ReturnsExistingTag()
        {
            // Arrange
            var tag = new Tag { Name = "Existing", Color = "#FF5733" };
            await _tagRepository.AddAsync(tag);

            // Act
            var result = await _tagService.GetOrCreateTagAsync("Existing", "#00FF00");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(tag.Id);
            result.Name.Should().Be("Existing");
            result.Color.Should().Be("#FF5733"); // Color should not be updated
            
            // Verify no new tag was created
            var allTags = await _tagRepository.GetAllAsync();
            allTags.Should().HaveCount(1);
        }

        [Test]
        public async Task GetOrCreateTagAsync_WithNewTag_CreatesNewTag()
        {
            // Arrange
            // No tags initially
            
            // Act
            var result = await _tagService.GetOrCreateTagAsync("New Tag", "#00FF00");

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Tag");
            result.Color.Should().Be("#00FF00");
            
            // Verify new tag was created
            var allTags = await _tagRepository.GetAllAsync();
            allTags.Should().HaveCount(1);
            allTags.First().Name.Should().Be("New Tag");
        }

        [Test]
        public async Task GetOrCreateTagAsync_WithNewTagNoColor_CreatesNewTagWithDefaultColor()
        {
            // Arrange
            // No tags initially
            
            // Act
            var result = await _tagService.GetOrCreateTagAsync("New Tag");

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Tag");
            result.Color.Should().Be("#808080"); // Default color
        }

        [Test]
        public async Task GetMostUsedTagsAsync_ReturnsTagsSortedByUsageCount()
        {
            // Arrange
            var tag1 = new Tag { Id = 1, Name = "Tag1" };
            var tag2 = new Tag { Id = 2, Name = "Tag2" };
            var tag3 = new Tag { Id = 3, Name = "Tag3" };
            
            await _tagRepository.AddAsync(tag1);
            await _tagRepository.AddAsync(tag2);
            await _tagRepository.AddAsync(tag3);

            // Tag1 has 3 bookmarks, Tag2 has 1 bookmark, Tag3 has 2 bookmarks
            await _bookmarkTagRepository.AddAsync(new BookmarkTag { BookmarkId = 1, TagId = 1 });
            await _bookmarkTagRepository.AddAsync(new BookmarkTag { BookmarkId = 2, TagId = 1 });
            await _bookmarkTagRepository.AddAsync(new BookmarkTag { BookmarkId = 3, TagId = 1 });
            await _bookmarkTagRepository.AddAsync(new BookmarkTag { BookmarkId = 1, TagId = 2 });
            await _bookmarkTagRepository.AddAsync(new BookmarkTag { BookmarkId = 2, TagId = 3 });
            await _bookmarkTagRepository.AddAsync(new BookmarkTag { BookmarkId = 3, TagId = 3 });

            // Act
            var result = await _tagService.GetMostUsedTagsAsync(2);

            // Assert
            result.Should().HaveCount(2);
            result.First().Id.Should().Be(1); // Tag1 should be first (3 uses)
            result.Skip(1).First().Id.Should().Be(3); // Tag3 should be second (2 uses)
        }

        [Test]
        public async Task GetMostUsedTagsAsync_WithNoTags_ReturnsEmptyList()
        {
            // Arrange
            // No tags
            
            // Act
            var result = await _tagService.GetMostUsedTagsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public void MergeTagsAsync_WithNonexistentSourceTag_ThrowsKeyNotFoundException()
        {
            // Arrange
            var targetTag = new Tag { Id = 2, Name = "Target" };
            _tagRepository.AddAsync(targetTag).Wait();

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => _tagService.MergeTagsAsync(999, 2));
            exception.Message.Should().Contain("Source tag with ID 999 not found");
        }

        [Test]
        public void MergeTagsAsync_WithNonexistentTargetTag_ThrowsKeyNotFoundException()
        {
            // Arrange
            var sourceTag = new Tag { Id = 1, Name = "Source" };
            _tagRepository.AddAsync(sourceTag).Wait();

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(() => _tagService.MergeTagsAsync(1, 999));
            exception.Message.Should().Contain("Target tag with ID 999 not found");
        }

        [Test]
        public void MergeTagsAsync_WithSameSourceAndTargetId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _tagService.MergeTagsAsync(1, 1));
            exception.Message.Should().Contain("Source and target tags cannot be the same");
        }

        [Test]
        public void MergeTagsAsync_WithInvalidSourceId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _tagService.MergeTagsAsync(0, 1));
            exception.Message.Should().Contain("Source tag ID must be greater than zero");
        }

        [Test]
        public void MergeTagsAsync_WithInvalidTargetId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _tagService.MergeTagsAsync(1, 0));
            exception.Message.Should().Contain("Target tag ID must be greater than zero");
        }
        
        [Test]
        public void MergeTagsAsync_SuccessfulMerge_MockService()
        {
            // To avoid the collection modification issue during enumeration, we'll create
            // a mock TagService with a controlled implementation of the dependencies
            
            // Arrange
            var mockTagRepo = Substitute.For<IRepository<Tag>>();
            var mockBookmarkTagRepo = Substitute.For<IRepository<BookmarkTag>>();
            
            var sourceTag = new Tag { Id = 1, Name = "Source" };
            var targetTag = new Tag { Id = 2, Name = "Target" };
            
            // Setup mocks
            mockTagRepo.GetByIdAsync(1).Returns(sourceTag);
            mockTagRepo.GetByIdAsync(2).Returns(targetTag);
            
            var sourceRelations = new List<BookmarkTag>
            {
                new BookmarkTag { Id = 1, BookmarkId = 1, TagId = 1 },
                new BookmarkTag { Id = 2, BookmarkId = 2, TagId = 1 }
            };
            
            mockBookmarkTagRepo.FindAsync(Arg.Any<Expression<Func<BookmarkTag, bool>>>()).Returns(
                callInfo => 
                {
                    var predicate = callInfo.Arg<Expression<Func<BookmarkTag, bool>>>();
                    var func = predicate.Compile();
                    
                    // For the source tag relations query
                    if (func(new BookmarkTag { TagId = 1 }))
                        return sourceRelations;
                    
                    // For the check of existing relations
                    return new List<BookmarkTag>();
                }
            );
            
            // Setup delete and add calls
            mockTagRepo.DeleteAsync(1).Returns(true);
            mockBookmarkTagRepo.AddAsync(Arg.Any<BookmarkTag>()).Returns(
                callInfo => callInfo.Arg<BookmarkTag>()
            );
            mockBookmarkTagRepo.DeleteAsync(Arg.Any<BookmarkTag>()).Returns(true);
            
            // Create the service with mocks
            var mockValidator = Substitute.For<IValidator<Tag>>();
            var tagService = new TagService(mockTagRepo, mockBookmarkTagRepo, mockValidator);
            
            // Act
            var result = tagService.MergeTagsAsync(1, 2).Result;
            
            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(2);
            result.Name.Should().Be("Target");
            
            // Verify source tag was deleted
            mockTagRepo.Received(1).DeleteAsync(1);
            
            // Verify bookmark relations were properly handled
            mockBookmarkTagRepo.Received(2).AddAsync(Arg.Is<BookmarkTag>(bt => bt.TagId == 2));
            mockBookmarkTagRepo.Received(2).DeleteAsync(Arg.Any<BookmarkTag>());
        }
        
        [Test]
        public void MergeTagsAsync_SkipsExistingRelations_MockService()
        {
            // Arrange
            var mockTagRepo = Substitute.For<IRepository<Tag>>();
            var mockBookmarkTagRepo = Substitute.For<IRepository<BookmarkTag>>();
            
            var sourceTag = new Tag { Id = 1, Name = "Source" };
            var targetTag = new Tag { Id = 2, Name = "Target" };
            
            // Setup mocks
            mockTagRepo.GetByIdAsync(1).Returns(sourceTag);
            mockTagRepo.GetByIdAsync(2).Returns(targetTag);
            
            var sourceRelations = new List<BookmarkTag>
            {
                new BookmarkTag { Id = 1, BookmarkId = 1, TagId = 1 },
                new BookmarkTag { Id = 2, BookmarkId = 2, TagId = 1 }
            };
            
            mockBookmarkTagRepo.FindAsync(Arg.Any<Expression<Func<BookmarkTag, bool>>>()).Returns(
                callInfo => 
                {
                    var predicate = callInfo.Arg<Expression<Func<BookmarkTag, bool>>>();
                    var func = predicate.Compile();
                    
                    // For the source tag relations query
                    if (func(new BookmarkTag { TagId = 1 }))
                        return sourceRelations;
                    
                    // For the check if bookmark 1 already has target tag
                    if (func(new BookmarkTag { BookmarkId = 1, TagId = 2 }))
                        return new List<BookmarkTag> { new BookmarkTag { Id = 3, BookmarkId = 1, TagId = 2 } };
                    
                    // For other queries
                    return new List<BookmarkTag>();
                }
            );
            
            // Setup delete and add calls
            mockTagRepo.DeleteAsync(1).Returns(true);
            mockBookmarkTagRepo.AddAsync(Arg.Any<BookmarkTag>()).Returns(
                callInfo => callInfo.Arg<BookmarkTag>()
            );
            mockBookmarkTagRepo.DeleteAsync(Arg.Any<BookmarkTag>()).Returns(true);
            
            // Create the service with mocks
            var mockValidator = Substitute.For<IValidator<Tag>>();
            var tagService = new TagService(mockTagRepo, mockBookmarkTagRepo, mockValidator);
            
            // Act
            var result = tagService.MergeTagsAsync(1, 2).Result;
            
            // Assert
            result.Should().NotBeNull();
            
            // Verify only one new relation was added (for bookmark 2, since bookmark 1 already has a relation)
            mockBookmarkTagRepo.Received(1).AddAsync(Arg.Is<BookmarkTag>(bt => bt.TagId == 2 && bt.BookmarkId == 2));
            
            // Both source relations should be deleted
            mockBookmarkTagRepo.Received(2).DeleteAsync(Arg.Any<BookmarkTag>());
        }
    }
}
