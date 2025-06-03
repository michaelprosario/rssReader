using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using rssReader.Models;

namespace rssReader.Services
{
    /// <summary>
    /// Interface for settings management.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets application settings.
        /// </summary>
        Task<AppSettings> GetSettingsAsync();
        
        /// <summary>
        /// Updates application settings.
        /// </summary>
        Task<AppSettings> UpdateSettingsAsync(AppSettings settings);
        
        /// <summary>
        /// Sets the global refresh interval.
        /// </summary>
        Task<int> SetGlobalRefreshIntervalAsync(int minutes);
        
        /// <summary>
        /// Gets the data directory path.
        /// </summary>
        Task<string> GetDataDirectoryPathAsync();
        
        /// <summary>
        /// Sets the data directory path.
        /// </summary>
        Task<string> SetDataDirectoryPathAsync(string path);
    }

    /// <summary>
    /// Implementation of settings service.
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly IDataStorageService _dataStorage;
        private const string SETTINGS_FILE = "settings.json";

        /// <summary>
        /// Constructor for SettingsService.
        /// </summary>
        public SettingsService(IDataStorageService dataStorage)
        {
            _dataStorage = dataStorage;
        }

        /// <inheritdoc/>
        public async Task<AppSettings> GetSettingsAsync()
        {
            var settings = await _dataStorage.LoadDataAsync<AppSettings>(SETTINGS_FILE);
            
            // Create default settings if none exist
            if (settings == null)
            {
                settings = new AppSettings();
                await _dataStorage.SaveDataAsync(SETTINGS_FILE, settings);
            }
            
            return settings;
        }

        /// <inheritdoc/>
        public async Task<AppSettings> UpdateSettingsAsync(AppSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            
            await _dataStorage.SaveDataAsync(SETTINGS_FILE, settings);
            return settings;
        }

        /// <inheritdoc/>
        public async Task<int> SetGlobalRefreshIntervalAsync(int minutes)
        {
            if (minutes < 1)
            {
                throw new ArgumentException("Refresh interval must be at least 1 minute");
            }
            
            var settings = await GetSettingsAsync();
            settings.GlobalRefreshIntervalMinutes = minutes;
            await UpdateSettingsAsync(settings);
            
            return minutes;
        }

        /// <inheritdoc/>
        public async Task<string> GetDataDirectoryPathAsync()
        {
            var settings = await GetSettingsAsync();
            return settings.DataDirectoryPath;
        }

        /// <inheritdoc/>
        public async Task<string> SetDataDirectoryPathAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Data directory path cannot be empty");
            }
            
            var settings = await GetSettingsAsync();
            settings.DataDirectoryPath = path;
            await UpdateSettingsAsync(settings);
            
            return path;
        }
    }
}
