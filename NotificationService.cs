using AdminSERMAC.Services;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using System;

public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly SQLiteService _sqliteService;
    private readonly int _stockMinimo;
    private readonly Timer _timer;

    public event EventHandler<NotificationEventArgs> NotificationReceived;

    public NotificationService(ILogger<NotificationService> logger, SQLiteService sqliteService, int stockMinimo = 10)
    {
        _logger = logger;
        _sqliteService = sqliteService;
        _stockMinimo = stockMinimo;

        // Revisar el stock cada hora
        _timer = new Timer(CheckStock, null, TimeSpan.Zero, TimeSpan.FromHours(1));
    }

    private async void CheckStock(object state)
    {
        try
        {
            using var connection = new SQLiteConnection(_sqliteService.connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(@"
                SELECT Codigo, Producto, Unidades 
                FROM Inventario 
                WHERE Unidades <= @stockMinimo", connection);

            command.Parameters.AddWithValue("@stockMinimo", _stockMinimo);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var notification = new NotificationEventArgs
                {
                    Type = NotificationType.StockBajo,
                    Message = $"Stock bajo para {reader["Producto"]}: {reader["Unidades"]} unidades",
                    Data = new
                    {
                        Codigo = reader["Codigo"].ToString(),
                        Producto = reader["Producto"].ToString(),
                        Unidades = Convert.ToInt32(reader["Unidades"])
                    }
                };

                NotificationReceived?.Invoke(this, notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revisar stock");
        }
    }

    public async Task CheckPagosPendientes()
    {
        try
        {
            using var connection = new SQLiteConnection(_sqliteService.connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(@"
                SELECT c.RUT, c.Nombre, v.FechaVenta, v.Total,
                       julianday('now') - julianday(v.FechaVenta) as DiasMora
                FROM Ventas v
                JOIN Clientes c ON v.RUT = c.RUT
                WHERE v.PagadoConCredito = 1
                AND julianday('now') - julianday(v.FechaVenta) > 30", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var notification = new NotificationEventArgs
                {
                    Type = NotificationType.PagoPendiente,
                    Message = $"Pago pendiente de {reader["Nombre"]}, monto: ${Convert.ToDouble(reader["Total"]):N0}",
                    Data = new
                    {
                        RUT = reader["RUT"].ToString(),
                        Nombre = reader["Nombre"].ToString(),
                        FechaVenta = Convert.ToDateTime(reader["FechaVenta"]),
                        Total = Convert.ToDouble(reader["Total"]),
                        DiasMora = Convert.ToInt32(reader["DiasMora"])
                    }
                };

                NotificationReceived?.Invoke(this, notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al revisar pagos pendientes");
        }
    }
}