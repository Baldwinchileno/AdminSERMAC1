using System;
using System.Collections.Generic;
using System.Data.SQLite;
using AdminSERMAC.Constants;
using AdminSERMAC.Exceptions;
using AdminSERMAC.Core.Interfaces;
using AdminSERMAC.Models;
using Microsoft.Extensions.Logging;

namespace AdminSERMAC.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly string connectionString;
        private readonly ILogger<ClienteRepository> _logger;

        public ClienteRepository(string connectionString, ILogger<ClienteRepository> logger)
        {
            this.connectionString = connectionString;
            this._logger = logger;
        }

        public List<Cliente> GetAll()
        {
            try
            {
                var clientes = new List<Cliente>();
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(QueryConstants.Cliente.SELECT_ALL, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                clientes.Add(MapClienteFromReader(reader));
                            }
                        }
                    }
                }
                _logger.LogInformation("Se obtuvieron {Count} clientes", clientes.Count);
                return clientes;
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Error al obtener todos los clientes");
                throw new ClienteException("Error al obtener la lista de clientes", ex);
            }
        }

        public async Task<double> CalcularDeudaTotalAsync(string rut)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand(QueryConstants.Cliente.CALCULAR_DEUDA_TOTAL, connection))
                    {
                        command.Parameters.AddWithValue("@RUT", rut);
                        return Convert.ToDouble(await command.ExecuteScalarAsync());
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Error al calcular deuda total del cliente: {RUT}", rut);
                throw new ClienteException($"Error al calcular deuda total del cliente con RUT: {rut}", ex);
            }
        }

        public async Task<IEnumerable<Cliente>> GetClientesConDeudaAsync()
        {
            try
            {
                var clientesConDeuda = new List<Cliente>();
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand(QueryConstants.Cliente.SELECT_CON_DEUDA, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                clientesConDeuda.Add(MapClienteFromReader(reader));
                            }
                        }
                    }
                }
                _logger.LogInformation("Se obtuvieron {Count} clientes con deuda", clientesConDeuda.Count);
                return clientesConDeuda;
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Error al obtener clientes con deuda");
                throw new ClienteException("Error al obtener la lista de clientes con deuda", ex);
            }
        }
        public Cliente GetByRUT(string rut)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(QueryConstants.Cliente.SELECT_BY_RUT, connection))
                    {
                        command.Parameters.AddWithValue("@RUT", rut);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var cliente = MapClienteFromReader(reader);
                                _logger.LogInformation("Cliente encontrado: {RUT}", rut);
                                return cliente;
                            }
                        }
                    }
                }
                _logger.LogWarning("Cliente no encontrado: {RUT}", rut);
                throw new ClienteNotFoundException(rut);
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Error al buscar cliente por RUT: {RUT}", rut);
                throw new ClienteException($"Error al buscar cliente con RUT: {rut}", ex);
            }
        }

        public void Add(Cliente cliente)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SQLiteCommand(QueryConstants.Cliente.INSERT, connection))
                            {
                                SetClienteParameters(command, cliente);
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            _logger.LogInformation("Cliente agregado: {RUT}", cliente.RUT);
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Error al agregar cliente: {RUT}", cliente.RUT);
                throw new ClienteException($"Error al agregar cliente con RUT: {cliente.RUT}", ex);
            }
        }

        public void Update(Cliente cliente)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SQLiteCommand(QueryConstants.Cliente.UPDATE, connection))
                            {
                                SetClienteParameters(command, cliente);
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    throw new ClienteNotFoundException(cliente.RUT);
                                }
                            }
                            transaction.Commit();
                            _logger.LogInformation("Cliente actualizado: {RUT}", cliente.RUT);
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente: {RUT}", cliente.RUT);
                throw new ClienteException($"Error al actualizar cliente con RUT: {cliente.RUT}", ex);
            }
        }

        public void Delete(string rut)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SQLiteCommand(QueryConstants.Cliente.DELETE, connection))
                            {
                                command.Parameters.AddWithValue("@RUT", rut);
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    throw new ClienteNotFoundException(rut);
                                }
                            }
                            transaction.Commit();
                            _logger.LogInformation("Cliente eliminado: {RUT}", rut);
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente: {RUT}", rut);
                throw new ClienteException($"Error al eliminar cliente con RUT: {rut}", ex);
            }
        }

        public void UpdateDeuda(string rut, double monto)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SQLiteCommand(QueryConstants.Cliente.UPDATE_DEUDA, connection))
                            {
                                command.Parameters.AddWithValue("@RUT", rut);
                                command.Parameters.AddWithValue("@Monto", monto);
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    throw new ClienteNotFoundException(rut);
                                }
                            }
                            transaction.Commit();
                            _logger.LogInformation("Deuda actualizada para cliente: {RUT}, Monto: {Monto}", rut, monto);
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Error al actualizar deuda del cliente: {RUT}", rut);
                throw new ClienteException($"Error al actualizar deuda del cliente con RUT: {rut}", ex);
            }
        }

        public List<Venta> GetVentasPorCliente(string rut)
        {
            try
            {
                var ventas = new List<Venta>();
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(QueryConstants.Cliente.SELECT_VENTAS, connection))
                    {
                        command.Parameters.AddWithValue("@RUT", rut);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ventas.Add(MapVentaFromReader(reader));
                            }
                        }
                    }
                }
                _logger.LogInformation("Ventas obtenidas para cliente: {RUT}, Cantidad: {Count}", rut, ventas.Count);
                return ventas;
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del cliente: {RUT}", rut);
                throw new ClienteException($"Error al obtener ventas del cliente con RUT: {rut}", ex);
            }
        }

        private Cliente MapClienteFromReader(SQLiteDataReader reader)
        {
            return new Cliente
            {
                RUT = reader["RUT"].ToString(),
                Nombre = reader["Nombre"].ToString(),
                Direccion = reader["Direccion"].ToString(),
                Giro = reader["Giro"].ToString(),
                Deuda = Convert.ToDouble(reader["Deuda"])
            };
        }

        private Venta MapVentaFromReader(SQLiteDataReader reader)
        {
            return new Venta
            {
                NumeroGuia = Convert.ToInt32(reader["NumeroGuia"]),
                CodigoProducto = reader["CodigoProducto"].ToString(),
                Descripcion = reader["Descripcion"].ToString(),
                Bandejas = Convert.ToInt32(reader["Bandejas"]),
                KilosNeto = Convert.ToDouble(reader["KilosNeto"]),
                FechaVenta = reader["FechaVenta"].ToString(),
                PagadoConCredito = Convert.ToInt32(reader["PagadoConCredito"]),
                RUT = reader["RUT"].ToString(),
                ClienteNombre = reader["ClienteNombre"].ToString(),
                Total = Convert.ToDouble(reader["Total"])
            };
        }

        private void SetClienteParameters(SQLiteCommand command, Cliente cliente)
        {
            command.Parameters.AddWithValue("@RUT", cliente.RUT);
            command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
            command.Parameters.AddWithValue("@Direccion", cliente.Direccion);
            command.Parameters.AddWithValue("@Giro", cliente.Giro);
            command.Parameters.AddWithValue("@Deuda", cliente.Deuda);
        }
    }
}