using ClosedXML.Excel;
using System.Collections.Generic;

public class Report
{
    public string Titulo { get; set; }
    public Dictionary<string, DataTable> Sections { get; private set; } = new Dictionary<string, DataTable>();
    public DateTime FechaGeneracion { get; private set; } = DateTime.Now;

    public void AddSection(string titulo, DataTable data)
    {
        Sections[titulo] = data;
    }

    public async Task ExportToExcel(string filePath)
    {
        using var workbook = new XLWorkbook();

        foreach (var section in Sections)
        {
            var worksheet = workbook.Worksheets.Add(section.Key);
            worksheet.Cell(1, 1).Value = section.Key;
            worksheet.Cell(1, 1).Style.Font.Bold = true;

            worksheet.Cell(3, 1).InsertTable(section.Value);
            worksheet.Columns().AdjustToContents();
        }

        await Task.Run(() => workbook.SaveAs(filePath));
    }
}
