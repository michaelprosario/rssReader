using AppCore.Models.Settings;
using AppCore.Repositories;
using FluentValidation;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppCore.Services.Settings
{
    /// <summary>
    /// Implementation of the AppSettings service
    /// </summary>
    public class AppSettingsService : IAppSettingsService
    {
        private readonly IRepository<AppSettings> _settingsRepository;
        private readonly IValidator<AppSettings>? _validator;
        private readonly Guid DefaultSettingsId = Guid.Parse("11111111-1111-1111-1111-111111111111"); 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settingsRepository">Repository for settings data access</param>
        /// <param name="validator">Validator for settings entities</param>
        public AppSettingsService(
            IRepository<AppSettings> settingsRepository,
            IValidator<AppSettings>? validator = null)
        {
            _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
            _validator = validator;
        }

        /// <summary>
        /// Validate settings using FluentValidation if validator is available
        /// </summary>
        /// <param name="settings">Settings to validate</param>
        /// <exception cref="ValidationException">Thrown if validation fails</exception>
        protected virtual void ValidateSettings(AppSettings settings)
        {
            if (_validator != null)
            {
                var validationResult = _validator.Validate(settings);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }
        }

        /// <summary>
        /// Get the current application settings
        /// </summary>
        /// <returns>The current settings</returns>
        public async Task<AppSettings> GetSettingsAsync()
        {
            // Try to get existing settings
            var settings = await _settingsRepository.GetByIdAsync(DefaultSettingsId);
            
            // If not found, create default settings
            if (settings == null)
            {
                settings = await CreateDefaultSettingsAsync();
            }
            
            return settings;
        }

        /// <summary>
        /// Update the application settings
        /// </summary>
        /// <param name="settings">The updated settings</param>
        /// <returns>The updated settings</returns>
        public async Task<AppSettings> UpdateSettingsAsync(AppSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            
            ValidateSettings(settings);
            
            // Make sure we're updating the same settings record
            settings.Id = DefaultSettingsId;
            settings.UpdatedAt = DateTime.UtcNow;
            
            // Check if settings exist
            var existingSettings = await _settingsRepository.GetByIdAsync(DefaultSettingsId);
            if (existingSettings == null)
            {
                // Create if not exist
                return await _settingsRepository.AddAsync(settings);
            }
            
            // Update existing settings
            return await _settingsRepository.UpdateAsync(settings);
        }

        /// <summary>
        /// Update the global refresh interval
        /// </summary>
        /// <param name="minutes">The new interval in minutes</param>
        /// <returns>The updated settings</returns>
        public async Task<AppSettings> UpdateGlobalRefreshIntervalAsync(int minutes)
        {
            if (minutes <= 0)
                throw new ArgumentException("Refresh interval must be greater than zero", nameof(minutes));
            
            var settings = await GetSettingsAsync();
            settings.GlobalRefreshIntervalMinutes = minutes;
            settings.UpdatedAt = DateTime.UtcNow;
            
            return await UpdateSettingsAsync(settings);
        }

        /// <summary>
        /// Update the auto-fetch full content setting
        /// </summary>
        /// <param name="enabled">Whether to auto-fetch full content</param>
        /// <returns>The updated settings</returns>
        public async Task<AppSettings> UpdateAutoFetchFullContentAsync(bool enabled)
        {
            var settings = await GetSettingsAsync();
            settings.AutoFetchFullContent = enabled;
            settings.UpdatedAt = DateTime.UtcNow;
            
            return await UpdateSettingsAsync(settings);
        }

        /// <summary>
        /// Reset settings to default values
        /// </summary>
        /// <returns>The default settings</returns>
        public async Task<AppSettings> ResetToDefaultsAsync()
        {
            var existingSettings = await _settingsRepository.GetByIdAsync(DefaultSettingsId);
            if (existingSettings != null)
            {
                await _settingsRepository.DeleteAsync(existingSettings);
            }
            
            return await CreateDefaultSettingsAsync();
        }
        
        /// <summary>
        /// Create default settings
        /// </summary>
        /// <returns>The created default settings</returns>
        private async Task<AppSettings> CreateDefaultSettingsAsync()
        {
            var settings = new AppSettings
            {
                Id = DefaultSettingsId,
                GlobalRefreshIntervalMinutes = 60,
                AutoFetchFullContent = true,
                AutoMarkAsReadSeconds = 5,
                UseReaderModeByDefault = false,
                MaxArticlesPerFeed = 100,
                ShowArticlePreviews = true,
                MarkAsReadOnScroll = true,
                CreatedAt = DateTime.UtcNow
            };
            
            return await _settingsRepository.AddAsync(settings);
        }
    }
}
