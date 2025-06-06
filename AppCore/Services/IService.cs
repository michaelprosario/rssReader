using AppCore.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AppCore.Services
{
    /// <summary>
    /// Generic service interface for entity operations
    /// </summary>
    /// <typeparam name="T">Entity type that implements IEntity</typeparam>
    public interface IService<T> where T : IEntity
    {
        /// <summary>
        /// Get entity by Id
        /// </summary>
        /// <param name="id">Entity Id</param>
        /// <returns>Entity if found, otherwise null</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns>IEnumerable of entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Get entities based on a filter predicate
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>Filtered entities</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Add a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Added entity</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Add multiple entities
        /// </summary>
        /// <param name="entities">Entities to add</param>
        /// <returns>Added entities</returns>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>Updated entity</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Delete an entity by Id
        /// </summary>
        /// <param name="id">Id of the entity to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <returns>True if deleted</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Check if entity with given predicate exists
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>True if entity exists, otherwise false</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}
