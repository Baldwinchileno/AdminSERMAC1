namespace AdminSERMAC.Constants
{
    public static class QueryConstants
    {
        public static class Cliente
        {
            public const string SELECT_ALL = @"
                SELECT * FROM Clientes";

            public const string SELECT_BY_RUT = @"
                SELECT * FROM Clientes 
                WHERE RUT = @RUT";

            public const string INSERT = @"
                INSERT INTO Clientes (
                    RUT, 
                    Nombre, 
                    Direccion, 
                    Giro, 
                    Deuda
                ) VALUES (
                    @RUT, 
                    @Nombre, 
                    @Direccion, 
                    @Giro, 
                    @Deuda
                )";

            public const string UPDATE = @"
                UPDATE Clientes 
                SET Nombre = @Nombre,
                    Direccion = @Direccion,
                    Giro = @Giro,
                    Deuda = @Deuda
                WHERE RUT = @RUT";

            public const string DELETE = @"
                DELETE FROM Clientes 
                WHERE RUT = @RUT";

            public const string UPDATE_DEUDA = @"
                UPDATE Clientes 
                SET Deuda = Deuda + @Monto 
                WHERE RUT = @RUT";

            public const string CALCULAR_DEUDA_TOTAL = "SELECT SUM(Deuda) FROM Clientes WHERE RUT = @RUT";
            public const string SELECT_VENTAS = @"
                SELECT 
                    v.NumeroGuia, 
                    v.CodigoProducto, 
                    v.Descripcion, 
                    v.Bandejas, 
                    v.KilosNeto, 
                    v.FechaVenta, 
                    v.PagadoConCredito,
                    v.RUT, 
                    c.Nombre as ClienteNombre,
                    v.Total
                FROM Ventas v
                JOIN Clientes c ON v.RUT = c.RUT
                WHERE v.RUT = @RUT";
        }
    }
}
