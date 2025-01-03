using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using AdminSERMAC.Core.Interfaces;
using AdminSERMAC.Repositories;
using Microsoft.Extensions.Logging;

namespace AdminSERMAC.Core.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly string _connectionString;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;

        // Repositorios
        private IClienteRepository _clienteRepository;
        private IProductoRepository _productoRepository;
        private IVentaRepository _ventaRepository;
        private IInventarioRepository _inventarioRepository;

        public UnitOfWork(string connectionString, ILogger<UnitOfWork> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        // Propiedades de repositorios
        public IClienteRepository Clientes => _clienteRepository ??= new ClienteRepository(GetConnection(), _logger);
        public IProductoRepository Productos => _productoRepository ??= new ProductoRepository(GetConnection(), _logger);
        public IVentaRepository Ventas => _ventaRepository ??= new VentaRepository(GetConnection(), _logger);
        public IInventarioRepository Inventario => _inventarioRepository ??= new InventarioRepository(GetConnection(), _logger);

        public bool HasActiveTransaction => _transaction != null;

        private IDbConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SQLiteConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        public IDbTransaction BeginTransaction()
        {
            try
            {
                if (_transaction != null)
                {
                    return _transaction;
                }

                _transaction = GetConnection().BeginTransaction();
                _logger.LogInformation("Transaction started");
                return _transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting transaction");
                throw;
            }
        }

        public async Task BeginTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    return;
                }

                await Task.Run(() => {
                    _transaction = GetConnection().BeginTransaction();
                });
                _logger.LogInformation("Transaction started asynchronously");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting async transaction");
                throw;
            }
        }

        public void Commit()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No active transaction to commit");
                }

                _transaction.Commit();
                _logger.LogInformation("Transaction committed");
                _transaction.Dispose();
                _transaction = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction");
                throw;
            }
        }

        public async Task CommitAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No active transaction to commit");
                }

                await Task.Run(() => {
                    _transaction.Commit();
                    _transaction.Dispose();
                    _transaction = null;
                });
                _logger.LogInformation("Transaction committed asynchronously");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing async transaction");
                throw;
            }
        }

        public void Rollback()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No active transaction to rollback");
                }

                _transaction.Rollback();
                _logger.LogInformation("Transaction rolled back");
                _transaction.Dispose();
                _transaction = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back transaction");
                throw;
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No active transaction to rollback");
                }

                await Task.Run(() => {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                });
                _logger.LogInformation("Transaction rolled back asynchronously");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back async transaction");
                throw;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await CommitAsync();
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes asynchronously");
                throw;
            }
        }

        public int SaveChanges()
        {
            try
            {
                if (_transaction != null)
                {
                    Commit();
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }
                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                _disposed = true;
            }
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}
