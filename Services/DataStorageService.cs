using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using rssReader.Models;

namespace rssReader.Services
{
    /// <summary>
    /// Interface for data storage operations.
    /// </summary>
    public interface IDataStorageService
    {
        /// <summary>
        /// Loads data of type T from a file.
        /// </summary>
        Task<T> LoadDataAsync<T>(string fileName) where T : class;
        
        /// <summary>
        /// Saves data of type T to a file.
        /// </summary>
        Task SaveDataAsync<T>(string fileName, T data) where T : class;
        
        /// <summary>
        /// Ensures the data directory exists.
        /// </summary>
        Task EnsureDataDirectoryExistsAsync();
        
        /// <summary>
        /// Gets the full path to a data file.
        /// </summary>
        string GetDataFilePath(string fileName);
    }

    /// <summary>
    /// Implementation of data storage service using JSON files.
    /// </summary>
    public class JsonFileStorageService : IDataStorageService
    {
        private readonly string _dataDirectory;
        
        /// <summary>
        /// Constructor for JsonFileStorageService.
        /// </summary>
        public JsonFileStorageService(string dataDirectory = null)
        {
            _dataDirectory = dataDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RSSReader"
            );
        }
        
        /// <inheritdoc/>
        public async Task<T> LoadDataAsync<T>(string fileName) where T : class
        {
            await EnsureDataDirectoryExistsAsync();
            
            var filePath = GetDataFilePath(fileName);
            
            if (!File.Exists(filePath))
            {
                return default(T);
            }
            
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading data from {fileName}: {ex.Message}");
                return default(T);
            }
        }
        
        /// <inheritdoc/>
        public async Task SaveDataAsync<T>(string fileName, T data) where T : class
        {
            await EnsureDataDirectoryExistsAsync();
            
            var filePath = GetDataFilePath(fileName);
            
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error saving data to {fileName}: {ex.Message}");
                throw;
            }
        }
        
        /// <inheritdoc/>
        public Task EnsureDataDirectoryExistsAsync()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
            
            return Task.CompletedTask;
        }
        
        /// <inheritdoc/>
        public string GetDataFilePath(string fileName)
        {
            return Path.Combine(_dataDirectory, fileName);
        }
    }
}
