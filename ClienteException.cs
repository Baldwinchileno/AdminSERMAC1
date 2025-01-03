using System;

namespace AdminSERMAC.Exceptions
{
    public class ClienteException : Exception
    {
        public ClienteException(string message) : base(message)
        {
        }

        public ClienteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class ClienteNotFoundException : ClienteException
    {
        public string RUT { get; }

        public ClienteNotFoundException(string rut)
            : base($"No se encontró el cliente con RUT: {rut}")
        {
            RUT = rut;
        }
    }

    public class ClienteDuplicadoException : ClienteException
    {
        public string RUT { get; }

        public ClienteDuplicadoException(string rut)
            : base($"Ya existe un cliente con el RUT: {rut}")
        {
            RUT = rut;
        }
    }

    public class ClienteConVentasException : ClienteException
    {
        public string RUT { get; }

        public ClienteConVentasException(string rut)
            : base($"No se puede eliminar el cliente con RUT: {rut} porque tiene ventas asociadas")
        {
            RUT = rut;
        }
    }

    public class ClienteDeudaException : ClienteException
    {
        public string RUT { get; }
        public double Monto { get; }

        public ClienteDeudaException(string rut, double monto)
            : base($"Error al actualizar la deuda del cliente con RUT: {rut}. Monto: {monto}")
        {
            RUT = rut;
            Monto = monto;
        }
    }

    public class ClienteValidationException : ClienteException
    {
        public ClienteValidationException(string message) : base(message)
        {
        }
    }
}
