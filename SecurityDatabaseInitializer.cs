using System.Data.SQLite;

public class SecurityDatabaseInitializer
{
    private readonly string _connectionString;

    public SecurityDatabaseInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeSecurityTables()
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        // Tabla de usuarios
        await ExecuteCommandAsync(connection, @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Email TEXT,
                Role TEXT NOT NULL,
                LastLogin TEXT,
                IsActive INTEGER DEFAULT 1
            )");

        // Tabla de registro de auditoría
        await ExecuteCommandAsync(connection, @"
            CREATE TABLE IF NOT EXISTS AuditLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Action TEXT NOT NULL,
                Username TEXT NOT NULL,
                Details TEXT,
                Timestamp TEXT NOT NULL,
                IpAddress TEXT,
                Result TEXT
            )");

        // Tabla de sesiones
        await ExecuteCommandAsync(connection, @"
            CREATE TABLE IF NOT EXISTS Sessions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                Token TEXT NOT NULL,
                Created TEXT NOT NULL,
                Expires TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            )");
    }

    private async Task ExecuteCommandAsync(SQLiteConnection connection, string commandText)
    {
        using var command = new SQLiteCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();
    }
}