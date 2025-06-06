using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppCore.Models;
using AppCore.Repositories;

namespace AppCore.UnitTests.Mocks
{
    /// <summary>
    /// In-memory repository implementation for testing
    /// </summary>
    public class MockRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly Dictionary<int, T> _entities = new Dictionary<int, T>();
        private int _nextId = 1;

        public Task<T> AddAsync(T entity)
        {
            if (entity.Id <= 0)
            {
                entity.Id = _nextId++;
            }
            else if (entity.Id >= _nextId)
            {
                _nextId = entity.Id + 1;
            }

            _entities[entity.Id] = entity;
            return Task.FromResult(entity);
        }

        public Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            var result = new List<T>();
            foreach (var entity in entities)
            {
                var addedEntity = AddAsync(entity).Result;
                result.Add(addedEntity);
            }
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<int> CountAsync()
        {
            return Task.FromResult(_entities.Count);
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            return Task.FromResult(_entities.Values.Count(compiledPredicate));
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(_entities.Remove(id));
        }

        public Task<bool> DeleteAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            return DeleteAsync(entity.Id);
        }

        public Task<int> DeleteRangeAsync(Expression<Func<T, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            var entitiesToDelete = _entities.Values.Where(compiledPredicate).ToList();
            var count = 0;

            foreach (var entity in entitiesToDelete)
            {
                if (_entities.Remove(entity.Id))
                {
                    count++;
                }
            }

            return Task.FromResult(count);
        }

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            return Task.FromResult(_entities.Values.Any(compiledPredicate));
        }

        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var compiledPredicate = predicate.Compile();
            return Task.FromResult(_entities.Values.Where(compiledPredicate));
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_entities.Values.AsEnumerable());
        }

        public Task<T?> GetByIdAsync(int id)
        {
            _entities.TryGetValue(id, out var entity);
            return Task.FromResult(entity);
        }

        public Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            if (entity.Id <= 0 || !_entities.ContainsKey(entity.Id))
                throw new KeyNotFoundException($"Entity with ID {entity.Id} not found");
            
            _entities[entity.Id] = entity;
            return Task.FromResult(entity);
        }
    }
}
