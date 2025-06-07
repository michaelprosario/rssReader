using AppCore.Models;
using AppCore.Repositories;
using AppInfra.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AppInfra.Repositories
{
    /// <summary>
    /// Entity Framework Core repository implementation
    /// </summary>
    /// <typeparam name="T">Entity type that implements IEntity</typeparam>
    public class EfCoreRepository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Database context</param>
        public EfCoreRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Get entity by Id
        /// </summary>
        /// <param name="id">Entity Id</param>
        /// <returns>Entity if found, otherwise null</returns>
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns>IEnumerable of entities</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Get entities based on a filter predicate
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>Filtered entities</returns>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Check if entity with given predicate exists
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>True if entity exists, otherwise false</returns>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        /// <summary>
        /// Add a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Added entity</returns>
        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.Id <= 0)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
            
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            return entity;
        }

        /// <summary>
        /// Add multiple entities
        /// </summary>
        /// <param name="entities">Entities to add</param>
        /// <returns>Added entities</returns>
        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));
            
            var now = DateTime.UtcNow;
            
            foreach (var entity in entities)
            {
                if (entity.Id <= 0)
                {
                    entity.CreatedAt = now;
                }
                entity.UpdatedAt = now;
            }
            
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            
            return entities;
        }

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>Updated entity</returns>
        public async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            entity.UpdatedAt = DateTime.UtcNow;
            
            _context.Entry(entity).State = EntityState.Modified;
            _context.Entry(entity).Property(e => e.CreatedAt).IsModified = false; // Don't change the creation date
            
            await _context.SaveChangesAsync();
            
            return entity;
        }

        /// <summary>
        /// Delete an entity by Id
        /// </summary>
        /// <param name="id">Id of the entity to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;
            
            return await DeleteAsync(entity);
        }

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <returns>True if deleted</returns>
        public async Task<bool> DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Delete entities that match the predicate
        /// </summary>
        /// <param name="predicate">Filter condition for entities to delete</param>
        /// <returns>Number of deleted entities</returns>
        public async Task<int> DeleteRangeAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await FindAsync(predicate);
            var entitiesToDelete = entities.ToList();
            
            if (!entitiesToDelete.Any())
                return 0;
            
            _dbSet.RemoveRange(entitiesToDelete);
            await _context.SaveChangesAsync();
            
            return entitiesToDelete.Count;
        }

        /// <summary>
        /// Count all entities
        /// </summary>
        /// <returns>Count of entities</returns>
        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        /// <summary>
        /// Count entities that match the predicate
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>Count of filtered entities</returns>
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }
    }
}
