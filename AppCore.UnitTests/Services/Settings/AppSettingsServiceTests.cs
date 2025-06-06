using AppCore.Models.Settings;
using AppCore.Repositories;
using AppCore.Services.Settings;
using AppCore.UnitTests.Mocks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace AppCore.UnitTests.Services.Settings
{
    [TestFixture]
    public class AppSettingsServiceTests
    {
        private IRepository<AppSettings> _settingsRepository;
        private IValidator<AppSettings> _settingsValidator;
        private AppSettingsService _settingsService;
        private const int DefaultSettingsId = 1;

        [SetUp]
        public void Setup()
        {
            // Initialize repository
            _settingsRepository = new MockRepository<AppSettings>();
            
            // Create mock validator
            _settingsValidator = Substitute.For<IValidator<AppSettings>>();
            
            // Default setup for validator - validation passes by default
            _settingsValidator.Validate(Arg.Any<AppSettings>())
                .Returns(new ValidationResult());
            
            // Create service
            _settingsService = new AppSettingsService(_settingsRepository, _settingsValidator);
        }

        [Test]
        public async Task GetSettingsAsync_WithNoExistingSettings_CreatesDefaultSettings()
        {
            // Arrange
            // No settings in repository

            // Act
            var result = await _settingsService.GetSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(DefaultSettingsId);
            result.GlobalRefreshIntervalMinutes.Should().Be(60); // Default value
            
            // Verify settings were created in repository
            var repoSettings = await _settingsRepository.GetByIdAsync(DefaultSettingsId);
            repoSettings.Should().NotBeNull();
        }

        [Test]
        public async Task GetSettingsAsync_WithExistingSettings_ReturnsExistingSettings()
        {
            // Arrange
            var existingSettings = new AppSettings
            {
                Id = DefaultSettingsId,
                GlobalRefreshIntervalMinutes = 30,
                AutoFetchFullContent = false
            };
            await _settingsRepository.AddAsync(existingSettings);

            // Act
            var result = await _settingsService.GetSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(DefaultSettingsId);
            result.GlobalRefreshIntervalMinutes.Should().Be(30);
            result.AutoFetchFullContent.Should().BeFalse();
        }

        [Test]
        public async Task UpdateSettingsAsync_WithValidSettings_UpdatesSettings()
        {
            // Arrange
            var existingSettings = new AppSettings
            {
                Id = DefaultSettingsId,
                GlobalRefreshIntervalMinutes = 60,
                AutoFetchFullContent = true
            };
            await _settingsRepository.AddAsync(existingSettings);

            var updatedSettings = new AppSettings
            {
                Id = DefaultSettingsId,
                GlobalRefreshIntervalMinutes = 15,
                AutoFetchFullContent = false,
                UseReaderModeByDefault = true
            };

            // Act
            var result = await _settingsService.UpdateSettingsAsync(updatedSettings);

            // Assert
            result.Should().NotBeNull();
            result.GlobalRefreshIntervalMinutes.Should().Be(15);
            result.AutoFetchFullContent.Should().BeFalse();
            result.UseReaderModeByDefault.Should().BeTrue();
            
            // Verify settings were updated in repository
            var repoSettings = await _settingsRepository.GetByIdAsync(DefaultSettingsId);
            repoSettings.Should().NotBeNull();
            repoSettings!.GlobalRefreshIntervalMinutes.Should().Be(15);
            repoSettings.AutoFetchFullContent.Should().BeFalse();
            repoSettings.UseReaderModeByDefault.Should().BeTrue();
        }

        [Test]
        public void UpdateSettingsAsync_WithInvalidSettings_ThrowsValidationException()
        {
            // Arrange
            var invalidSettings = new AppSettings
            {
                Id = DefaultSettingsId,
                GlobalRefreshIntervalMinutes = -10 // Invalid value
            };
            
            // Setup validator to fail
            var validationFailure = new ValidationFailure("GlobalRefreshIntervalMinutes", "Must be greater than 0");
            var validationResult = new ValidationResult(new[] { validationFailure });
            _settingsValidator.Validate(Arg.Any<AppSettings>()).Returns(validationResult);

            // Act & Assert
            Assert.ThrowsAsync<ValidationException>(() => 
                _settingsService.UpdateSettingsAsync(invalidSettings));
        }

        [Test]
        public async Task UpdateGlobalRefreshIntervalAsync_WithValidValue_UpdatesInterval()
        {
            // Arrange
            var existingSettings = new AppSettings
            {
                Id = DefaultSettingsId,
                GlobalRefreshIntervalMinutes = 60
            };
            await _settingsRepository.AddAsync(existingSettings);

            // Act
            var result = await _settingsService.UpdateGlobalRefreshIntervalAsync(15);

            // Assert
            result.Should().NotBeNull();
            result.GlobalRefreshIntervalMinutes.Should().Be(15);
            
            // Verify settings were updated in repository
            var repoSettings = await _settingsRepository.GetByIdAsync(DefaultSettingsId);
            repoSettings.Should().NotBeNull();
            repoSettings!.GlobalRefreshIntervalMinutes.Should().Be(15);
        }

        [Test]
        public async Task UpdateAutoFetchFullContentAsync_UpdatesSetting()
        {
            // Arrange
            var existingSettings = new AppSettings
            {
                Id = DefaultSettingsId,
                AutoFetchFullContent = true
            };
            await _settingsRepository.AddAsync(existingSettings);

            // Act
            var result = await _settingsService.UpdateAutoFetchFullContentAsync(false);

            // Assert
            result.Should().NotBeNull();
            result.AutoFetchFullContent.Should().BeFalse();
            
            // Verify settings were updated in repository
            var repoSettings = await _settingsRepository.GetByIdAsync(DefaultSettingsId);
            repoSettings.Should().NotBeNull();
            repoSettings!.AutoFetchFullContent.Should().BeFalse();
        }

        [Test]
        public async Task ResetToDefaultsAsync_ResetsAllSettings()
        {
            // Arrange
            var customizedSettings = new AppSettings
            {
                Id = DefaultSettingsId,
                GlobalRefreshIntervalMinutes = 15,
                AutoFetchFullContent = false,
                UseReaderModeByDefault = true,
                MaxArticlesPerFeed = 50
            };
            await _settingsRepository.AddAsync(customizedSettings);

            // Act
            var result = await _settingsService.ResetToDefaultsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(DefaultSettingsId);
            result.GlobalRefreshIntervalMinutes.Should().Be(60); // Default value
            result.AutoFetchFullContent.Should().BeTrue(); // Default value
            result.UseReaderModeByDefault.Should().BeFalse(); // Default value
            result.MaxArticlesPerFeed.Should().Be(100); // Default value
            
            // Verify settings were reset in repository
            var repoSettings = await _settingsRepository.GetByIdAsync(DefaultSettingsId);
            repoSettings.Should().NotBeNull();
            repoSettings!.GlobalRefreshIntervalMinutes.Should().Be(60);
            repoSettings.AutoFetchFullContent.Should().BeTrue();
        }
    }
}
