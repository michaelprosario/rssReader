using AppCore.Models;
using AppCore.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AppCore.Services
{
    /// <summary>
    /// Base implementation of the generic service interface
    /// </summary>
    /// <typeparam name="T">Entity type that implements IEntity</typeparam>
    public abstract class ServiceBase<T> : IService<T> where T : IEntity
    {
        protected readonly IRepository<T> _repository;
        protected readonly IValidator<T>? _validator;

        /// <summary>
        /// Constructor with repository and optional validator
        /// </summary>
        /// <param name="repository">Repository for data access</param>
        /// <param name="validator">Optional validator for entity validation</param>
        protected ServiceBase(IRepository<T> repository, IValidator<T>? validator = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _validator = validator;
        }

        /// <summary>
        /// Validate entity using FluentValidation if validator is available
        /// </summary>
        /// <param name="entity">Entity to validate</param>
        /// <exception cref="ValidationException">Thrown if validation fails</exception>
        protected virtual void ValidateEntity(T entity)
        {
            if (_validator != null)
            {
                var validationResult = _validator.Validate(entity);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }
        }

        /// <summary>
        /// Add a new entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <returns>Added entity</returns>
        public virtual async Task<T> AddAsync(T entity)
        {
            ValidateEntity(entity);
            return await _repository.AddAsync(entity);
        }

        /// <summary>
        /// Add multiple entities
        /// </summary>
        /// <param name="entities">Entities to add</param>
        /// <returns>Added entities</returns>
        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                ValidateEntity(entity);
            }
            return await _repository.AddRangeAsync(entities);
        }

        /// <summary>
        /// Delete an entity by Id
        /// </summary>
        /// <param name="id">Id of the entity to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        public virtual async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than zero", nameof(id));
            
            return await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <returns>True if deleted</returns>
        public virtual async Task<bool> DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            if (entity.Id <= 0)
                throw new ArgumentException("Entity must have a valid ID to delete", nameof(entity));
            
            return await _repository.DeleteAsync(entity);
        }

        /// <summary>
        /// Check if entity with given predicate exists
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>True if entity exists, otherwise false</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.ExistsAsync(predicate);
        }

        /// <summary>
        /// Get entities based on a filter predicate
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        /// <returns>Filtered entities</returns>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.FindAsync(predicate);
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns>IEnumerable of entities</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Get entity by Id
        /// </summary>
        /// <param name="id">Entity Id</param>
        /// <returns>Entity if found, otherwise null</returns>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than zero", nameof(id));
            
            return await _repository.GetByIdAsync(id);
        }

        /// <summary>
        /// Update an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <returns>Updated entity</returns>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            if (entity.Id <= 0)
                throw new ArgumentException("Entity must have a valid ID to update", nameof(entity));
            
            // Check if entity exists
            var existingEntity = await _repository.GetByIdAsync(entity.Id);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Entity with ID {entity.Id} not found");
            
            ValidateEntity(entity);
            
            // If the entity is a BaseEntity, update the UpdatedAt property
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.UpdatedAt = DateTime.UtcNow;
            }
            
            return await _repository.UpdateAsync(entity);
        }
    }
}
