using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System;

public class SecurityManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityManager> _logger;
    private readonly string _connectionString;

    public SecurityManager(IConfiguration configuration, ILogger<SecurityManager> logger, string connectionString)
    {
        _configuration = configuration;
        _logger = logger;
        _connectionString = connectionString;
    }

    // Modelo para registro de auditoría
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Username { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
    }

    // Autenticación
    public async Task<bool> AuthenticateUser(string username, string password)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var hashedPassword = HashPassword(password);
        var command = new SQLiteCommand(
            "SELECT COUNT(*) FROM Users WHERE Username = @username AND PasswordHash = @password",
            connection);

        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@password", hashedPassword);

        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
        await LogAction("LOGIN_ATTEMPT", username, $"Login attempt from user {username}",
            result > 0 ? "Success" : "Failed");

        return result > 0;
    }

    // Encriptación de datos sensibles
    public string EncryptData(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_configuration["EncryptionKey"]);
        aes.IV = new byte[16]; // IV fijo para este ejemplo

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(cipherBytes);
    }

    public string DecryptData(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_configuration["EncryptionKey"]);
        aes.IV = new byte[16];

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    // Logging de auditoría
    public async Task LogAction(string action, string username, string details, string result = null)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SQLiteCommand(@"
            INSERT INTO AuditLogs (Action, Username, Details, Timestamp, IpAddress, Result)
            VALUES (@action, @username, @details, @timestamp, @ipAddress, @result)",
            connection);

        command.Parameters.AddWithValue("@action", action);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@details", details);
        command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);
        command.Parameters.AddWithValue("@ipAddress", GetClientIpAddress());
        command.Parameters.AddWithValue("@result", result);

        await command.ExecuteNonQueryAsync();
    }

    // Sanitización de entrada
    public string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Eliminar caracteres potencialmente peligrosos
        var sanitized = Regex.Replace(input, @"[<>()&;]", "");

        // Prevenir SQL Injection
        sanitized = sanitized.Replace("'", "''");

        return sanitized;
    }

    // Validación de RUT chileno
    public bool ValidateRUT(string rut)
    {
        rut = rut.Replace(".", "").Replace("-", "");
        if (!Regex.IsMatch(rut, @"^\d{7,8}[0-9Kk]$")) return false;

        char dv = rut[^1];
        string numero = rut[..^1];

        int suma = 0;
        int multiplicador = 2;

        for (int i = numero.Length - 1; i >= 0; i--)
        {
            suma += int.Parse(numero[i].ToString()) * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        int resto = suma % 11;
        char dvCalculado = resto == 0 ? '0' : resto == 1 ? 'K' : (11 - resto).ToString()[0];

        return char.ToUpper(dv) == char.ToUpper(dvCalculado);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private string GetClientIpAddress()
    {
        // En una aplicación real, esto vendría del contexto HTTP
        return "127.0.0.1";
    }
}