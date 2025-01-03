using System.Collections.Generic;
using AdminSERMAC.Models;

namespace AdminSERMAC.Core.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<IEnumerable<Cliente>> GetClientesConDeudaAsync();
        Task<double> CalcularDeudaTotalAsync(string rut);
        Task ActualizarDeudaAsync(string rut, double monto);
        List<Cliente> GetAll();
        Cliente GetByRUT(string rut);
        void Add(Cliente cliente);
        void Update(Cliente cliente);
        void Delete(string rut);
        void UpdateDeuda(string rut, double monto);
        List<Venta> GetVentasPorCliente(string rut);
    }
}


