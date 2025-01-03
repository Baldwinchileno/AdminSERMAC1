using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AdminSERMAC.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Data;

namespace AdminSERMAC.Core.Base
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly string _connectionString;
        protected readonly ILogger<BaseRepository<T>> _logger;

        protected BaseRepository(string connectionString, ILogger<BaseRepository<T>> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        protected IDbConnection CreateConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public virtual async Task<T> GetByIdAsync(object id)
        {
            try
            {
                using var connection = CreateConnection();
                var query = GetByIdQuery();
                return await connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id });
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
                using var connection = CreateConnection();
                var query = GetAllQuery();
                return await connection.QueryAsync<T>(query);
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
                using var connection = CreateConnection();
                var query = GetFindQuery(predicate);
                return await connection.QueryAsync<T>(query);
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
                using var connection = CreateConnection();
                var query = GetInsertQuery();
                await connection.ExecuteAsync(query, entity);
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
                using var connection = CreateConnection();
                using var transaction = connection.BeginTransaction();
                try
                {
                    var query = GetInsertQuery();
                    await connection.ExecuteAsync(query, entities, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
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
                using var connection = CreateConnection();
                var query = GetUpdateQuery();
                await connection.ExecuteAsync(query, entity);
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
                using var connection = CreateConnection();
                var query = GetDeleteQuery();
                await connection.ExecuteAsync(query, entity);
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
                using var connection = CreateConnection();
                using var transaction = connection.BeginTransaction();
                try
                {
                    var query = GetDeleteQuery();
                    await connection.ExecuteAsync(query, entities, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting range of {typeof(T).Name}s");
                throw;
            }
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                using var connection = CreateConnection();
                var query = GetExistsQuery(predicate);
                return await connection.ExecuteScalarAsync<bool>(query);
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
                using var connection = CreateConnection();
                var query = GetCountQuery(predicate);
                return await connection.ExecuteScalarAsync<int>(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting {typeof(T).Name}s");
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
                using var connection = CreateConnection();
                var query = GetPagedQuery(pageIndex, pageSize, predicate, orderBy);
                var totalQuery = GetCountQuery(predicate);

                var multi = await connection.QueryMultipleAsync($"{totalQuery}; {query}");
                var total = await multi.ReadFirstAsync<int>();
                var items = await multi.ReadAsync<T>();

                return (items, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting paged {typeof(T).Name}s");
                throw;
            }
        }

        // Métodos abstractos que deben ser implementados por las clases derivadas
        protected abstract string GetByIdQuery();
        protected abstract string GetAllQuery();
        protected abstract string GetInsertQuery();
        protected abstract string GetUpdateQuery();
        protected abstract string GetDeleteQuery();
        protected abstract string GetFindQuery(Expression<Func<T, bool>> predicate);
        protected abstract string GetExistsQuery(Expression<Func<T, bool>> predicate);
        protected abstract string GetCountQuery(Expression<Func<T, bool>> predicate = null);
        protected abstract string GetPagedQuery(
            int pageIndex,
            int pageSize,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null);
    }
}