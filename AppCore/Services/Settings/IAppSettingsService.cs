using AppCore.Models.Settings;
using System.Threading.Tasks;

namespace AppCore.Services.Settings
{
    /// <summary>
    /// Interface for AppSettings service operations
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// Get the current application settings
        /// </summary>
        /// <returns>The current settings</returns>
        Task<AppSettings> GetSettingsAsync();

        /// <summary>
        /// Update the application settings
        /// </summary>
        /// <param name="settings">The updated settings</param>
        /// <returns>The updated settings</returns>
        Task<AppSettings> UpdateSettingsAsync(AppSettings settings);

        /// <summary>
        /// Update the global refresh interval
        /// </summary>
        /// <param name="minutes">The new interval in minutes</param>
        /// <returns>The updated settings</returns>
        Task<AppSettings> UpdateGlobalRefreshIntervalAsync(int minutes);

        /// <summary>
        /// Update the auto-fetch full content setting
        /// </summary>
        /// <param name="enabled">Whether to auto-fetch full content</param>
        /// <returns>The updated settings</returns>
        Task<AppSettings> UpdateAutoFetchFullContentAsync(bool enabled);

        /// <summary>
        /// Reset settings to default values
        /// </summary>
        /// <returns>The default settings</returns>
        Task<AppSettings> ResetToDefaultsAsync();
    }
}
