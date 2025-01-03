using AdminSERMAC.Services;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using System.Threading.Tasks;
using System;

public class BranchManager
{
    private readonly SQLiteService _sqliteService;
    private readonly ILogger<BranchManager> _logger;

    public BranchManager(SQLiteService sqliteService, ILogger<BranchManager> logger)
    {
        _sqliteService = sqliteService;
        _logger = logger;
    }

    public async Task CreateBranchTables()
    {
        try
        {
            using var connection = new SQLiteConnection(_sqliteService.connectionString);
            await connection.OpenAsync();

            // Tabla de sucursales
            await ExecuteCommandAsync(connection, @"
                CREATE TABLE IF NOT EXISTS Sucursales (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT NOT NULL,
                    Direccion TEXT NOT NULL,
                    Telefono TEXT,
                    Email TEXT,
                    Encargado TEXT
                )");

            // Tabla de inventario por sucursal
            await ExecuteCommandAsync(connection, @"
                CREATE TABLE IF NOT EXISTS InventarioSucursal (
                    SucursalId INTEGER,
                    Codigo TEXT,
                    Unidades INTEGER NOT NULL DEFAULT 0,
                    Kilos REAL NOT NULL DEFAULT 0,
                    PRIMARY KEY (SucursalId, Codigo),
                    FOREIGN KEY (SucursalId) REFERENCES Sucursales(Id),
                    FOREIGN KEY (Codigo) REFERENCES Productos(Codigo)
                )");

            // Tabla de traspasos entre sucursales
            await ExecuteCommandAsync(connection, @"
                CREATE TABLE IF NOT EXISTS Traspasos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SucursalOrigenId INTEGER,
                    SucursalDestinoId INTEGER,
                    Codigo TEXT,
                    Unidades INTEGER NOT NULL,
                    Kilos REAL NOT NULL,
                    FechaTraspaso TEXT NOT NULL,
                    Estado TEXT NOT NULL,
                    FOREIGN KEY (SucursalOrigenId) REFERENCES Sucursales(Id),
                    FOREIGN KEY (SucursalDestinoId) REFERENCES Sucursales(Id),
                    FOREIGN KEY (Codigo) REFERENCES Productos(Codigo)
                )");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando tablas de sucursales");
            throw;
        }
    }

    private async Task ExecuteCommandAsync(SQLiteConnection connection, string commandText)
    {
        using var command = new SQLiteCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CrearSucursal(Sucursal sucursal)
    {
        try
        {
            using var connection = new SQLiteConnection(_sqliteService.connectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(@"
                INSERT INTO Sucursales (Nombre, Direccion, Telefono, Email, Encargado)
                VALUES (@nombre, @direccion, @telefono, @email, @encargado);
                SELECT last_insert_rowid();", connection);

            command.Parameters.AddWithValue("@nombre", sucursal.Nombre);
            command.Parameters.AddWithValue("@direccion", sucursal.Direccion);
            command.Parameters.AddWithValue("@telefono", sucursal.Telefono);
            command.Parameters.AddWithValue("@email", sucursal.Email);
            command.Parameters.AddWithValue("@encargado", sucursal.Encargado);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando sucursal");
            throw;
        }
    }

    public async Task RealizarTraspaso(Traspaso traspaso)
    {
        using var connection = new SQLiteConnection(_sqliteService.connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Verificar stock en sucursal origen
            var stockOrigen = await VerificarStock(connection, traspaso.SucursalOrigenId,
                traspaso.Codigo, traspaso.Unidades, traspaso.Kilos);

            if (!stockOrigen)
            {
                throw new Exception("Stock insuficiente en sucursal origen");
            }

            // Registrar el traspaso
            await RegistrarTraspaso(connection, traspaso);

            // Actualizar inventario en ambas sucursales
            await ActualizarInventarioSucursal(connection, traspaso.SucursalOrigenId,
                traspaso.Codigo, -traspaso.Unidades, -traspaso.Kilos);

            await ActualizarInventarioSucursal(connection, traspaso.SucursalDestinoId,
                traspaso.Codigo, traspaso.Unidades, traspaso.Kilos);

            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    private async Task<bool> VerificarStock(SQLiteConnection connection, int sucursalId,
        string codigo, int unidades, double kilos)
    {
        var command = new SQLiteCommand(@"
            SELECT Unidades, Kilos 
            FROM InventarioSucursal 
            WHERE SucursalId = @sucursalId AND Codigo = @codigo", connection);

        command.Parameters.AddWithValue("@sucursalId", sucursalId);
        command.Parameters.AddWithValue("@codigo", codigo);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var stockUnidades = reader.GetInt32(0);
            var stockKilos = reader.GetDouble(1);
            return stockUnidades >= unidades && stockKilos >= kilos;
        }
        return false;
    }

    private async Task RegistrarTraspaso(SQLiteConnection connection, Traspaso traspaso)
    {
        var command = new SQLiteCommand(@"
            INSERT INTO Traspasos (
                SucursalOrigenId, SucursalDestinoId, Codigo, 
                Unidades, Kilos, FechaTraspaso, Estado)
            VALUES (
                @origen, @destino, @codigo, 
                @unidades, @kilos, @fecha, @estado)", connection);

        command.Parameters.AddWithValue("@origen", traspaso.SucursalOrigenId);
        command.Parameters.AddWithValue("@destino", traspaso.SucursalDestinoId);
        command.Parameters.AddWithValue("@codigo", traspaso.Codigo);
        command.Parameters.AddWithValue("@unidades", traspaso.Unidades);
        command.Parameters.AddWithValue("@kilos", traspaso.Kilos);
        command.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@estado", "Completado");

        await command.ExecuteNonQueryAsync();
    }

    private async Task ActualizarInventarioSucursal(SQLiteConnection connection,
        int sucursalId, string codigo, int unidades, double kilos)
    {
        var command = new SQLiteCommand(@"
            INSERT INTO InventarioSucursal (SucursalId, Codigo, Unidades, Kilos)
            VALUES (@sucursalId, @codigo, @unidades, @kilos)
            ON CONFLICT(SucursalId, Codigo) DO UPDATE SET
                Unidades = Unidades + @unidades,
                Kilos = Kilos + @kilos", connection);

        command.Parameters.AddWithValue("@sucursalId", sucursalId);
        command.Parameters.AddWithValue("@codigo", codigo);
        command.Parameters.AddWithValue("@unidades", unidades);
        command.Parameters.AddWithValue("@kilos", kilos);

        await command.ExecuteNonQueryAsync();
    }
}
