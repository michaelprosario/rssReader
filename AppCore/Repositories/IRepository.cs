using AppCore.Models;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    /// <summary>
    /// Generic repository interface for entity operations
    /// </summary>
    /// <typeparam name="T">Entity type that implements IEntity</typeparam>
    public interface IRepository<T> where T : IEntity
    {
        /// <summary>
        /// Get entity by Id
        /// </summary>
        /// <param name="id">Entity Id</param>
        /// <returns>Entity if found, otherwise null</returns>
        Task<T?> GetByIdAsync(Guid id);

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
        /// Check if entity with given predicate exists
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>True if entity exists, otherwise false</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

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
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <returns>True if deleted</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Delete entities that match the predicate
        /// </summary>
        /// <param name="predicate">Filter condition for entities to delete</param>
        /// <returns>Number of deleted entities</returns>
        Task<int> DeleteRangeAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get count of all entities
        /// </summary>
        /// <returns>Count of entities</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Get count of entities that match the predicate
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>Count of filtered entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}
