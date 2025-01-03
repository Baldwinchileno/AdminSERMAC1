using System.Collections.Generic;
using AdminSERMAC.Models;

namespace AdminSERMAC.Core.Interfaces
{
    public interface IClienteService
    {
        List<Cliente> ObtenerTodosLosClientes();
        void AgregarCliente(Cliente nuevoCliente);
        void ActualizarCliente(Cliente clienteActualizado);
        void EliminarCliente(string rut);
        double CalcularDeudaTotal(string rut);
        List<Venta> ObtenerVentasCliente(string rut);
        void ActualizarDeudaCliente(string rut, double monto);
    }
}

