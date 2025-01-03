using AdminSERMAC.Services.Reports;
using AdminSERMAC.Services;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using System.Threading.Tasks;
using System;

public class ReportGenerator
{
    private readonly SQLiteService _sqliteService;
    private readonly ILogger<ReportGenerator> _logger;

    public ReportGenerator(SQLiteService sqliteService, ILogger<ReportGenerator> logger)
    {
        _sqliteService = sqliteService;
        _logger = logger;
    }

    public async Task<Report> GenerateVentasReport(DateTime desde, DateTime hasta)
    {
        try
        {
            using var connection = new SQLiteConnection(_sqliteService.connectionString);
            await connection.OpenAsync();

            var report = new Report { Titulo = "Reporte de Ventas" };

            // Ventas totales
            var ventasTotales = await GetVentasTotales(connection, desde, hasta);
            report.AddSection("Ventas Totales", ventasTotales);

            // Ventas por producto
            var ventasPorProducto = await GetVentasPorProducto(connection, desde, hasta);
            report.AddSection("Ventas por Producto", ventasPorProducto);

            // Ventas por cliente
            var ventasPorCliente = await GetVentasPorCliente(connection, desde, hasta);
            report.AddSection("Ventas por Cliente", ventasPorCliente);

            // Métricas adicionales
            var metricas = await GetMetricas(connection, desde, hasta);
            report.AddSection("Métricas", metricas);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte de ventas");
            throw;
        }
    }

    private async Task<DataTable> GetVentasTotales(SQLiteConnection connection, DateTime desde, DateTime hasta)
    {
        var command = new SQLiteCommand(@"
            SELECT 
                strftime('%Y-%m', FechaVenta) as Mes,
                COUNT(*) as CantidadVentas,
                SUM(Total) as MontoTotal,
                SUM(CASE WHEN PagadoConCredito = 1 THEN Total ELSE 0 END) as VentasCredito,
                SUM(CASE WHEN PagadoConCredito = 0 THEN Total ELSE 0 END) as VentasContado
            FROM Ventas
            WHERE date(FechaVenta) BETWEEN @desde AND @hasta
            GROUP BY strftime('%Y-%m', FechaVenta)
            ORDER BY Mes", connection);

        command.Parameters.AddWithValue("@desde", desde.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@hasta", hasta.ToString("yyyy-MM-dd"));

        var dataTable = new DataTable();
        using var adapter = new SQLiteDataAdapter(command);
        adapter.Fill(dataTable);
        return dataTable;
    }

    private async Task<DataTable> GetVentasPorProducto(SQLiteConnection connection, DateTime desde, DateTime hasta)
    {
        var command = new SQLiteCommand(@"
            SELECT 
                v.CodigoProducto,
                v.Descripcion,
                COUNT(*) as CantidadVentas,
                SUM(v.KilosNeto) as KilosTotales,
                SUM(v.Total) as MontoTotal,
                AVG(v.Total) as PromedioVenta
            FROM Ventas v
            WHERE date(v.FechaVenta) BETWEEN @desde AND @hasta
            GROUP BY v.CodigoProducto, v.Descripcion
            ORDER BY MontoTotal DESC", connection);

        command.Parameters.AddWithValue("@desde", desde.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@hasta", hasta.ToString("yyyy-MM-dd"));

        var dataTable = new DataTable();
        using var adapter = new SQLiteDataAdapter(command);
        adapter.Fill(dataTable);
        return dataTable;
    }

    private async Task<DataTable> GetVentasPorCliente(SQLiteConnection connection, DateTime desde, DateTime hasta)
    {
        var command = new SQLiteCommand(@"
            SELECT 
                c.RUT,
                c.Nombre,
                COUNT(*) as CantidadCompras,
                SUM(v.Total) as MontoTotal,
                SUM(CASE WHEN v.PagadoConCredito = 1 THEN v.Total ELSE 0 END) as ComprasCredito,
                c.Deuda as DeudaActual
            FROM Ventas v
            JOIN Clientes c ON v.RUT = c.RUT
            WHERE date(v.FechaVenta) BETWEEN @desde AND @hasta
            GROUP BY c.RUT, c.Nombre
            ORDER BY MontoTotal DESC", connection);

        command.Parameters.AddWithValue("@desde", desde.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@hasta", hasta.ToString("yyyy-MM-dd"));

        var dataTable = new DataTable();
        using var adapter = new SQLiteDataAdapter(command);
        adapter.Fill(dataTable);
        return dataTable;
    }

    private async Task<DataTable> GetMetricas(SQLiteConnection connection, DateTime desde, DateTime hasta)
    {
        var command = new SQLiteCommand(@"
            SELECT
                (SELECT COUNT(DISTINCT RUT) FROM Ventas
                 WHERE date(FechaVenta) BETWEEN @desde AND @hasta) as ClientesUnicos,
                
                (SELECT COUNT(*) FROM Ventas 
                 WHERE date(FechaVenta) BETWEEN @desde AND @hasta) as TotalVentas,
                
                (SELECT SUM(Total) FROM Ventas 
                 WHERE date(FechaVenta) BETWEEN @desde AND @hasta) as MontoTotal,
                
                (SELECT AVG(Total) FROM Ventas 
                 WHERE date(FechaVenta) BETWEEN @desde AND @hasta) as PromedioVenta,
                
                (SELECT SUM(KilosNeto) FROM Ventas 
                 WHERE date(FechaVenta) BETWEEN @desde AND @hasta) as KilosTotales", connection);

        command.Parameters.AddWithValue("@desde", desde.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@hasta", hasta.ToString("yyyy-MM-dd"));

        var dataTable = new DataTable();
        using var adapter = new SQLiteDataAdapter(command);
        adapter.Fill(dataTable);
        return dataTable;
    }
}
