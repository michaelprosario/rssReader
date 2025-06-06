using AppCore.Models;
using System.Linq.Expressions;

namespace AppCore.Repositories
{
    /// <summary>
    /// Abstract base repository implementation that other repositories can derive from
    /// </summary>
    /// <typeparam name="T">Entity type that implements IEntity</typeparam>
    public abstract class RepositoryBase<T> : IRepository<T> where T : IEntity
    {
        /// <summary>
        /// Add a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Added entity</returns>
        public abstract Task<T> AddAsync(T entity);

        /// <summary>
        /// Add multiple entities
        /// </summary>
        /// <param name="entities">Entities to add</param>
        /// <returns>Added entities</returns>
        public abstract Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Get count of all entities
        /// </summary>
        /// <returns>Count of entities</returns>
        public abstract Task<int> CountAsync();

        /// <summary>
        /// Get count of entities that match the predicate
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>Count of filtered entities</returns>
        public abstract Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Delete an entity by Id
        /// </summary>
        /// <param name="id">Id of the entity to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        public abstract Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <returns>True if deleted</returns>
        public abstract Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Delete entities that match the predicate
        /// </summary>
        /// <param name="predicate">Filter condition for entities to delete</param>
        /// <returns>Number of deleted entities</returns>
        public abstract Task<int> DeleteRangeAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Check if entity with given predicate exists
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>True if entity exists, otherwise false</returns>
        public abstract Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get entities based on a filter predicate
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>Filtered entities</returns>
        public abstract Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns>IEnumerable of entities</returns>
        public abstract Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Get entity by Id
        /// </summary>
        /// <param name="id">Entity Id</param>
        /// <returns>Entity if found, otherwise null</returns>
        public abstract Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>Updated entity</returns>
        public abstract Task<T> UpdateAsync(T entity);
    }
}
