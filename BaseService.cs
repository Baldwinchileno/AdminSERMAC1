using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AdminSERMAC.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdminSERMAC.Core.Base
{
    public abstract class BaseService<T> where T : class
    {
        protected readonly IRepository<T> _repository;
        protected readonly ILogger<BaseService<T>> _logger;

        protected BaseService(IRepository<T> repository, ILogger<BaseService<T>> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public virtual async Task<T> GetByIdAsync(object id)
        {
            try
            {
                _logger.LogInformation($"Getting {typeof(T).Name} by id: {id}");
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting {typeof(T).Name} by id: {id}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation($"Getting all {typeof(T).Name}s");
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all {typeof(T).Name}s");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogInformation($"Finding {typeof(T).Name}s with predicate");
                return await _repository.FindAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding {typeof(T).Name}s with predicate");
                throw;
            }
        }

        public virtual async Task AddAsync(T entity)
        {
            try
            {
                await ValidateEntityAsync(entity);
                _logger.LogInformation($"Adding new {typeof(T).Name}");
                await _repository.AddAsync(entity);
                await OnEntityAddedAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    await ValidateEntityAsync(entity);
                }
                _logger.LogInformation($"Adding range of {typeof(T).Name}s");
                await _repository.AddRangeAsync(entities);
                foreach (var entity in entities)
                {
                    await OnEntityAddedAsync(entity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding range of {typeof(T).Name}s");
                throw;
            }
        }

        public virtual async Task UpdateAsync(T entity)
        {
            try
            {
                await ValidateEntityAsync(entity);
                _logger.LogInformation($"Updating {typeof(T).Name}");
                await _repository.UpdateAsync(entity);
                await OnEntityUpdatedAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task DeleteAsync(T entity)
        {
            try
            {
                await ValidateDeleteAsync(entity);
                _logger.LogInformation($"Deleting {typeof(T).Name}");
                await _repository.DeleteAsync(entity);
                await OnEntityDeletedAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    await ValidateDeleteAsync(entity);
                }
                _logger.LogInformation($"Deleting range of {typeof(T).Name}s");
                await _repository.DeleteRangeAsync(entities);
                foreach (var entity in entities)
                {
                    await OnEntityDeletedAsync(entity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting range of {typeof(T).Name}s");
                throw;
            }
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            try
            {
                _logger.LogInformation($"Getting paged {typeof(T).Name}s - Page: {pageIndex}, Size: {pageSize}");
                return await _repository.GetPagedAsync(pageIndex, pageSize, predicate, orderBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting paged {typeof(T).Name}s");
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.LogInformation($"Checking existence of {typeof(T).Name}");
                return await _repository.ExistsAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking existence of {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            try
            {
                _logger.LogInformation($"Counting {typeof(T).Name}s");
                return await _repository.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting {typeof(T).Name}s");
                throw;
            }
        }

        // Métodos protegidos que pueden ser sobrescritos por las clases derivadas
        protected virtual Task ValidateEntityAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{typeof(T).Name} cannot be null");
            }
            return Task.CompletedTask;
        }

        protected virtual Task ValidateDeleteAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"{typeof(T).Name} cannot be null");
            }
            return Task.CompletedTask;
        }

        // Eventos que pueden ser manejados por las clases derivadas
        protected virtual Task OnEntityAddedAsync(T entity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnEntityUpdatedAsync(T entity)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnEntityDeletedAsync(T entity)
        {
            return Task.CompletedTask;
        }

        // Métodos utilitarios protegidos
        protected virtual void LogInformation(string message)
        {
            _logger.LogInformation($"{typeof(T).Name} Service: {message}");
        }

        protected virtual void LogWarning(string message)
        {
            _logger.LogWarning($"{typeof(T).Name} Service: {message}");
        }

        protected virtual void LogError(Exception ex, string message)
        {
            _logger.LogError(ex, $"{typeof(T).Name} Service: {message}");
        }

        protected virtual async Task<bool> ValidateUniqueConstraintAsync(
            Expression<Func<T, bool>> predicate,
            string constraintName,
            T entity = null)
        {
            var existing = await _repository.FindAsync(predicate);
            var existingList = existing.ToList();

            if (!existingList.Any())
            {
                return true;
            }

            if (entity != null && existingList.Count == 1)
            {
                // Si estamos actualizando y solo existe una entidad, verificar si es la misma
                var existingEntity = existingList.First();
                if (existingEntity.Equals(entity))
                {
                    return true;
                }
            }

            LogWarning($"Unique constraint violation: {constraintName}");
            return false;
        }
    }
}