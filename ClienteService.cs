using System;
using System.Collections.Generic;
using System.Linq;
using AdminSERMAC.Exceptions;
using AdminSERMAC.Interfaces;
using AdminSERMAC.Models;
using Microsoft.Extensions.Logging;

namespace AdminSERMAC.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(IClienteRepository clienteRepository, ILogger<ClienteService> logger)
        {
            _clienteRepository = clienteRepository;
            _logger = logger;
        }

        public List<Cliente> ObtenerTodosLosClientes()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de todos los clientes");
                return _clienteRepository.GetAll();
            }
            catch (ClienteException ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de clientes");
                throw;
            }
        }

        public Cliente ObtenerClientePorRUT(string rut)
        {
            ValidarRUT(rut);
            try
            {
                _logger.LogInformation("Buscando cliente con RUT: {RUT}", rut);
                return _clienteRepository.GetByRUT(rut);
            }
            catch (ClienteException ex)
            {
                _logger.LogError(ex, "Error al obtener cliente con RUT: {RUT}", rut);
                throw;
            }
        }

        public void AgregarCliente(Cliente cliente)
        {
            ValidarCliente(cliente);
            try
            {
                // Verificar si ya existe un cliente con ese RUT
                try
                {
                    var clienteExistente = _clienteRepository.GetByRUT(cliente.RUT);
                    if (clienteExistente != null)
                    {
                        throw new ClienteDuplicadoException(cliente.RUT);
                    }
                }
                catch (ClienteNotFoundException)
                {
                    // Es correcto que no exista el cliente
                }

                _logger.LogInformation("Agregando nuevo cliente con RUT: {RUT}", cliente.RUT);
                _clienteRepository.Add(cliente);
            }
            catch (ClienteException ex)
            {
                _logger.LogError(ex, "Error al agregar cliente con RUT: {RUT}", cliente.RUT);
                throw;
            }
        }

        public void ActualizarCliente(Cliente cliente)
        {
            ValidarCliente(cliente);
            try
            {
                _logger.LogInformation("Actualizando cliente con RUT: {RUT}", cliente.RUT);
                _clienteRepository.Update(cliente);
            }
            catch (ClienteException ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente con RUT: {RUT}", cliente.RUT);
                throw;
            }
        }

        public void EliminarCliente(string rut)
        {
            ValidarRUT(rut);
            try
            {
                // Verificar si el cliente tiene ventas
                var ventas = _clienteRepository.GetVentasPorCliente(rut);
                if (ventas.Any())
                {
                    throw new ClienteConVentasException(rut);
                }

                _logger.LogInformation("Eliminando cliente con RUT: {RUT}", rut);
                _clienteRepository.Delete(rut);
            }
            catch (ClienteException ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente con RUT: {RUT}", rut);
                throw;
            }
        }

        public void ActualizarDeudaCliente(string rut, double monto)
        {
            ValidarRUT(rut);
            if (monto == 0)
            {
                throw new ClienteValidationException("El monto no puede ser cero");
            }

            try
            {
                _logger.LogInformation("Actualizando deuda del cliente {RUT}. Monto: {Monto}", rut, monto);
                _clienteRepository.UpdateDeuda(rut, monto);
            }
            catch (ClienteException ex)
            {
                _logger.LogError(ex, "Error al actualizar deuda del cliente {RUT}. Monto: {Monto}", rut, monto);
                throw;
            }
        }

        public List<Venta> ObtenerVentasCliente(string rut)
        {
            ValidarRUT(rut);
            try
            {
                _logger.LogInformation("Obteniendo ventas del cliente: {RUT}", rut);
                return _clienteRepository.GetVentasPorCliente(rut);
            }
            catch (ClienteException ex)
            {
                _logger.LogError(ex, "Error al obtener ventas del cliente: {RUT}", rut);
                throw;
            }
        }

        public double CalcularDeudaTotal(string rut)
        {
            ValidarRUT(rut);
            try
            {
                _logger.LogInformation("Calculando deuda total del cliente: {RUT}", rut);
                var cliente = _clienteRepository.GetByRUT(rut);
                var ventas = _clienteRepository.GetVentasPorCliente(rut);

                double deudaTotal = cliente.Deuda;
                foreach (var venta in ventas.Where(v => v.PagadoConCredito == 1))
                {
                    deudaTotal += venta.Total;
                }

                _logger.LogInformation("Deuda total calculada para cliente {RUT}: {DeudaTotal}", rut, deudaTotal);
                return deudaTotal;
            }
            catch (ClienteException ex)
            {
                _logger.LogError(ex, "Error al calcular deuda total del cliente: {RUT}", rut);
                throw;
            }
        }

        private void ValidarCliente(Cliente cliente)
        {
            if (cliente == null)
            {
                throw new ClienteValidationException("El cliente no puede ser nulo");
            }

            ValidarRUT(cliente.RUT);

            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                throw new ClienteValidationException("El nombre del cliente es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(cliente.Direccion))
            {
                throw new ClienteValidationException("La dirección del cliente es obligatoria");
            }

            if (string.IsNullOrWhiteSpace(cliente.Giro))
            {
                throw new ClienteValidationException("El giro del cliente es obligatorio");
            }

            if (cliente.Deuda < 0)
            {
                throw new ClienteValidationException("La deuda no puede ser negativa");
            }
        }

        private void ValidarRUT(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut))
            {
                throw new ClienteValidationException("El RUT es obligatorio");
            }

            // Aquí podrías agregar validación del formato del RUT chileno
            // Por ejemplo, usando una expresión regular
            // if (!Regex.IsMatch(rut, @"^(\d{1,2}\.\d{3}\.\d{3}[-][0-9kK]{1})$"))
            // {
            //     throw new ClienteValidationException("El formato del RUT no es válido");
            // }
        }
    }
}