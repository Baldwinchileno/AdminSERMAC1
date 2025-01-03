using AdminSERMAC.Services;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.IO.Compression;

public class FileDataManager
{
    private readonly string _backupPath;
    private readonly ILogger<FileDataManager> _logger;
    private readonly SQLiteService _sqliteService;

    public FileDataManager(string backupPath, ILogger<FileDataManager> logger, SQLiteService sqliteService)
    {
        _backupPath = backupPath;
        _logger = logger;
        _sqliteService = sqliteService;
    }

    public async Task<bool> ValidateCSVFormat(string filePath)
    {
        try
        {
            using (var reader = new StreamReader(filePath))
            {
                // Leer encabezados
                var headers = (await reader.ReadLineAsync())?.Split(',');
                if (headers == null || !ValidateRequiredHeaders(headers))
                {
                    return false;
                }

                // Validar algunas líneas de datos
                int lineCount = 0;
                while (!reader.EndOfStream && lineCount < 5)
                {
                    var line = await reader.ReadLineAsync();
                    if (!ValidateDataLine(line))
                    {
                        return false;
                    }
                    lineCount++;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando formato CSV");
            return false;
        }
    }

    private bool ValidateRequiredHeaders(string[] headers)
    {
        var requiredHeaders = new[] { "Codigo", "Nombre", "Precio", "Stock" };
        return requiredHeaders.All(rh => headers.Contains(rh));
    }

    private bool ValidateDataLine(string line)
    {
        var fields = line.Split(',');
        return fields.Length >= 4 &&
               !string.IsNullOrEmpty(fields[0]) &&
               decimal.TryParse(fields[2], out _);
    }

    public async Task<string> CreateBackup()
    {
        try
        {
            var backupFileName = $"backup_{DateTime.Now:yyyyMMddHHmmss}.db";
            var backupFilePath = Path.Combine(_backupPath, backupFileName);

            using (var connection = new SQLiteConnection(_sqliteService.connectionString))
            {
                await connection.OpenAsync();
                var backup = new SQLiteBackup(connection, "main", new SQLiteConnection($"Data Source={backupFilePath}"), "main");
                backup.Step(-1);
                backup.Dispose();
            }

            // Comprimir el backup
            var compressedPath = Path.ChangeExtension(backupFilePath, ".zip");
            using (var zip = ZipFile.Open(compressedPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(backupFilePath, backupFileName);
            }

            File.Delete(backupFilePath);
            return compressedPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando backup");
            throw;
        }
    }

    public async Task ExportData(string format, string filePath, DataTable data)
    {
        switch (format.ToLower())
        {
            case "csv":
                await ExportToCSV(filePath, data);
                break;
            case "excel":
                await ExportToExcel(filePath, data);
                break;
            case "json":
                await ExportToJSON(filePath, data);
                break;
            default:
                throw new ArgumentException("Formato no soportado");
        }
    }

    private async Task ExportToCSV(string filePath, DataTable data)
    {
        using (var writer = new StreamWriter(filePath))
        {
            // Escribir headers
            await writer.WriteLineAsync(string.Join(",", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));

            // Escribir datos
            foreach (DataRow row in data.Rows)
            {
                var fields = row.ItemArray.Select(field =>
                    field.ToString().Contains(",") ? $"\"{field}\"" : field.ToString());
                await writer.WriteLineAsync(string.Join(",", fields));
            }
        }
    }

    private async Task ExportToExcel(string filePath, DataTable data)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Data");
            worksheet.Cell(1, 1).InsertTable(data);
            await Task.Run(() => workbook.SaveAs(filePath));
        }
    }

    private async Task ExportToJSON(string filePath, DataTable data)
    {
        var rows = new List<Dictionary<string, object>>();
        foreach (DataRow row in data.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in data.Columns)
            {
                dict[col.ColumnName] = row[col];
            }
            rows.Add(dict);
        }

        var json = JsonSerializer.Serialize(rows, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(filePath, json);
    }
}