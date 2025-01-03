using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using AdminSERMAC.Models;
using AdminSERMAC.Services;
using ClosedXML.Excel;
using System.Data.SQLite;

namespace AdminSERMAC.Forms
{
    public class VentasForm : Form
    {
        private Label numeroGuiaLabel;
        private TextBox numeroGuiaTextBox;
        private Label rutLabel;
        private ComboBox rutComboBox;
        private Label clienteLabel;
        private TextBox clienteTextBox;
        private Label direccionLabel;
        private TextBox direccionTextBox;
        private Label giroLabel;
        private TextBox giroTextBox;
        private Label fechaEmisionLabel;
        private DateTimePicker fechaEmisionPicker;
        private Label totalVentaLabel;
        private TextBox totalVentaTextBox;

        private DataGridView ventasDataGridView;
        private Button finalizarButton;
        private Button imprimirButton;
        private Button exportarExcelButton;
        private Button cancelarButton;
        private CheckBox pagarConCreditoCheckBox;

        private SQLiteService sqliteService;
        private double totalVenta = 0;

        public VentasForm()
        {
            this.Text = "Gestión de Ventas";
            this.Width = 1000;
            this.Height = 800;

            sqliteService = new SQLiteService();

            InitializeComponents();
            ConfigureEvents();
        }

        private void InitializeComponents()
        {
            // Número de Guía
            numeroGuiaLabel = new Label() { Text = "Número de Guía", Top = 20, Left = 20, Width = 120 };
            numeroGuiaTextBox = new TextBox() { Top = 20, Left = 150, Width = 200, ReadOnly = true };
            numeroGuiaTextBox.Text = sqliteService.GetUltimoNumeroGuia().ToString();

            // RUT Cliente
            rutLabel = new Label() { Text = "RUT Cliente", Top = 50, Left = 20, Width = 120 };
            rutComboBox = new ComboBox() { Top = 50, Left = 150, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            var clientes = sqliteService.GetClientes();
            if (clientes.Count > 0)
            {
                rutComboBox.DataSource = clientes;
                rutComboBox.DisplayMember = "RUT";
                rutComboBox.ValueMember = "RUT";
            }
            else
            {
                MessageBox.Show("No se encontraron clientes en la base de datos.");
            }

            // Cliente
            clienteLabel = new Label() { Text = "Cliente", Top = 80, Left = 20, Width = 120 };
            clienteTextBox = new TextBox() { Top = 80, Left = 150, Width = 200, ReadOnly = true };

            // Dirección
            direccionLabel = new Label() { Text = "Dirección", Top = 110, Left = 20, Width = 120 };
            direccionTextBox = new TextBox() { Top = 110, Left = 150, Width = 200, ReadOnly = true };

            // Giro
            giroLabel = new Label() { Text = "Giro Comercial", Top = 140, Left = 20, Width = 120 };
            giroTextBox = new TextBox() { Top = 140, Left = 150, Width = 200, ReadOnly = true };

            // Fecha de Emisión
            fechaEmisionLabel = new Label() { Text = "Fecha de Emisión", Top = 170, Left = 20, Width = 120 };
            fechaEmisionPicker = new DateTimePicker() { Top = 170, Left = 150, Width = 200 };

            // Total Venta
            totalVentaLabel = new Label()
            {
                Text = "Total Venta:",
                Top = 600,
                Left = 600,
                Width = 100,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            totalVentaTextBox = new TextBox()
            {
                Top = 600,
                Left = 700,
                Width = 150,
                ReadOnly = true,
                Font = new Font(this.Font, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Right
            };

            // Tabla de ventas
            ventasDataGridView = new DataGridView()
            {
                Top = 210,
                Left = 20,
                Width = 950,
                Height = 380,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Configurar columnas del DataGridView
            ventasDataGridView.Columns.Add("Codigo", "Código");
            ventasDataGridView.Columns.Add("Descripcion", "Descripción");
            ventasDataGridView.Columns.Add("Unidades", "Unidades");
            ventasDataGridView.Columns.Add("Bandejas", "Bandejas");
            ventasDataGridView.Columns.Add("KilosBruto", "Kilos Bruto");
            ventasDataGridView.Columns.Add("KilosNeto", "Kilos Neto");
            ventasDataGridView.Columns.Add("Precio", "Precio");
            ventasDataGridView.Columns.Add("Total", "Total");
            ventasDataGridView.Columns.Add("CantidadExistente", "Stock Disponible");

            // Botones
            finalizarButton = new Button()
            {
                Text = "Finalizar Venta",
                Top = 630,
                Left = 20,
                Width = 150,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };

            imprimirButton = new Button()
            {
                Text = "Imprimir Factura",
                Top = 630,
                Left = 180,
                Width = 150
            };

            exportarExcelButton = new Button()
            {
                Text = "Exportar a Excel",
                Top = 630,
                Left = 340,
                Width = 150
            };

            cancelarButton = new Button()
            {
                Text = "Cancelar Venta",
                Top = 630,
                Left = 500,
                Width = 150,
                BackColor = Color.IndianRed,
                ForeColor = Color.White
            };

            // CheckBox Pagar con Crédito
            pagarConCreditoCheckBox = new CheckBox()
            {
                Text = "Pagar con Crédito",
                Top = 600,
                Left = 20,
                Width = 150
            };

            // Agregar controles al formulario
            this.Controls.AddRange(new Control[] {
                numeroGuiaLabel, numeroGuiaTextBox,
                rutLabel, rutComboBox,
                clienteLabel, clienteTextBox,
                direccionLabel, direccionTextBox,
                giroLabel, giroTextBox,
                fechaEmisionLabel, fechaEmisionPicker,
                totalVentaLabel, totalVentaTextBox,
                ventasDataGridView,
                finalizarButton, imprimirButton, exportarExcelButton, cancelarButton,
                pagarConCreditoCheckBox
            });
        }

        private void ConfigureEvents()
        {
            ventasDataGridView.CellEndEdit += VentasDataGridView_CellEndEdit;
            ventasDataGridView.CellValidating += VentasDataGridView_CellValidating;
            cancelarButton.Click += CancelarButton_Click;
            finalizarButton.Click += FinalizarButton_Click;
            imprimirButton.Click += ImprimirButton_Click;
            exportarExcelButton.Click += ExportarExcelButton_Click;
            rutComboBox.SelectedIndexChanged += RutComboBox_SelectedIndexChanged;
        }

        private void RutComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedRUT = rutComboBox.SelectedValue?.ToString();
            if (selectedRUT != null)
            {
                var cliente = sqliteService.GetClientePorRUT(selectedRUT);
                if (cliente != null)
                {
                    clienteTextBox.Text = cliente.Nombre;
                    direccionTextBox.Text = cliente.Direccion;
                    giroTextBox.Text = cliente.Giro;
                }
                else
                {
                    LimpiarDatosCliente();
                    MessageBox.Show("Cliente no encontrado.");
                }
            }
        }

        private void VentasDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == ventasDataGridView.Columns["Codigo"].Index)
            {
                string codigo = e.FormattedValue.ToString();
                if (!string.IsNullOrEmpty(codigo))
                {
                    var producto = sqliteService.GetProductoPorCodigo(codigo);
                    if (producto == null)
                    {
                        e.Cancel = true;
                        MessageBox.Show("Producto no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            else if (e.ColumnIndex == ventasDataGridView.Columns["Unidades"].Index)
            {
                if (int.TryParse(e.FormattedValue.ToString(), out int unidades))
                {
                    string codigo = ventasDataGridView.Rows[e.RowIndex].Cells["Codigo"].Value?.ToString();
                    if (!string.IsNullOrEmpty(codigo))
                    {
                        var producto = sqliteService.GetProductoPorCodigo(codigo);
                        if (producto != null && unidades > producto.Unidades)
                        {
                            e.Cancel = true;
                            MessageBox.Show($"Stock insuficiente. Stock disponible: {producto.Unidades}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
        }

        private void VentasDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == ventasDataGridView.Columns["Codigo"].Index)
            {
                ActualizarDatosProducto(e.RowIndex);
            }
            else if (e.ColumnIndex == ventasDataGridView.Columns["Bandejas"].Index ||
                     e.ColumnIndex == ventasDataGridView.Columns["KilosBruto"].Index ||
                     e.ColumnIndex == ventasDataGridView.Columns["Precio"].Index)
            {
                CalcularTotales(e.RowIndex);
            }
        }

        private void ActualizarDatosProducto(int rowIndex)
        {
            string codigo = ventasDataGridView.Rows[rowIndex].Cells["Codigo"].Value?.ToString();
            if (!string.IsNullOrEmpty(codigo))
            {
                var producto = sqliteService.GetProductoPorCodigo(codigo);
                if (producto != null)
                {
                    ventasDataGridView.Rows[rowIndex].Cells["Descripcion"].Value = producto.Nombre;
                    ventasDataGridView.Rows[rowIndex].Cells["CantidadExistente"].Value = producto.Unidades;
                }
            }
        }

        private void CalcularTotales(int rowIndex)
        {
            try
            {
                int bandejas = int.TryParse(ventasDataGridView.Rows[rowIndex].Cells["Bandejas"].Value?.ToString(), out int b) ? b : 0;
                double kilosBruto = double.TryParse(ventasDataGridView.Rows[rowIndex].Cells["KilosBruto"].Value?.ToString(), out double k) ? k : 0;
                double precio = double.TryParse(ventasDataGridView.Rows[rowIndex].Cells["Precio"].Value?.ToString(), out double p) ? p : 0;

                double kilosNeto = kilosBruto - (1.5 * bandejas);
                if (kilosNeto < 0) kilosNeto = 0;

                ventasDataGridView.Rows[rowIndex].Cells["KilosNeto"].Value = kilosNeto.ToString("N2");
                double totalFila = kilosNeto * precio;
                ventasDataGridView.Rows[rowIndex].Cells["Total"].Value = totalFila.ToString("N0");

                CalcularTotalVenta();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular totales: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalcularTotalVenta()
        {
            totalVenta = 0;
            foreach (DataGridViewRow row in ventasDataGridView.Rows)
            {
                if (!row.IsNewRow && double.TryParse(row.Cells["Total"].Value?.ToString(), out double total))
                {
                    totalVenta += total;
                }
            }
            totalVentaTextBox.Text = totalVenta.ToString("C0");
        }

        private void FinalizarButton_Click(object sender, EventArgs e)
        {
            if (!ValidarVenta()) return;

            try
            {
                using (var connection = sqliteService.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            int numeroGuia = int.Parse(numeroGuiaTextBox.Text);
                            string fechaVenta = fechaEmisionPicker.Value.ToString("yyyy-MM-dd");
                            string rutCliente = rutComboBox.SelectedValue.ToString();
                            string clienteNombre = clienteTextBox.Text;

                            foreach (DataGridViewRow row in ventasDataGridView.Rows)
                            {
                                if (row.IsNewRow) continue;

                                if (!ValidarStockDisponible(row))
                                {
                                    transaction.Rollback();
                                    return;
                                }

                                RegistrarVenta(numeroGuia, rutCliente, clienteNombre, row, fechaVenta, transaction);
                            }

                            if (pagarConCreditoCheckBox.Checked)
                            {
                                sqliteService.ActualizarDeudaCliente(rutCliente, totalVenta, transaction);
                            }

                            sqliteService.IncrementarNumeroGuia(transaction);
                            transaction.Commit();

                            MessageBox.Show("Venta finalizada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LimpiarFormulario();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception($"Error al procesar la venta: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarVenta()
        {
            if (rutComboBox.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un cliente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (ventasDataGridView.Rows.Count <= 1)
            {
                MessageBox.Show("Debe agregar al menos un producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (totalVenta <= 0)
            {
                MessageBox.Show("El total de la venta debe ser mayor que cero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool ValidarStockDisponible(DataGridViewRow row)
        {
            string codigo = row.Cells["Codigo"].Value?.ToString();
            if (int.TryParse(row.Cells["Unidades"].Value?.ToString(), out int unidades))
            {
                var producto = sqliteService.GetProductoPorCodigo(codigo);
                if (producto != null && unidades > producto.Unidades)
                {
                    MessageBox.Show($"Stock insuficiente para el producto {codigo}.\nStock disponible: {producto.Unidades}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }

        private void RegistrarVenta(int numeroGuia, string rutCliente, string clienteNombre,
            DataGridViewRow row, string fechaVenta, SQLiteTransaction transaction)
        {
            string codigo = row.Cells["Codigo"].Value?.ToString();
            string descripcion = row.Cells["Descripcion"].Value?.ToString();
            int unidades = int.Parse(row.Cells["Unidades"].Value?.ToString());
            int bandejas = int.Parse(row.Cells["Bandejas"].Value?.ToString());
            double kilosNeto = double.Parse(row.Cells["KilosNeto"].Value?.ToString());

            sqliteService.DescontarInventario(codigo, unidades, kilosNeto, transaction);

            sqliteService.AgregarVenta(new Venta
            {
                NumeroGuia = numeroGuia,
                CodigoProducto = codigo,
                Descripcion = descripcion,
                Bandejas = bandejas,
                KilosNeto = kilosNeto,
                FechaVenta = fechaVenta,
                PagadoConCredito = pagarConCreditoCheckBox.Checked ? 1 : 0,
                RUT = rutCliente,
                ClienteNombre = clienteNombre,
                Total = double.Parse(row.Cells["Total"].Value?.ToString())
            }, transaction);
        }

        private void CancelarButton_Click(object sender, EventArgs e)
        {
            if (ventasDataGridView.Rows.Count > 1)
            {
                var resultado = MessageBox.Show(
                    "¿Está seguro que desea cancelar la venta actual?",
                    "Confirmar Cancelación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    LimpiarFormulario();
                }
            }
            else
            {
                LimpiarFormulario();
            }
        }

        private void LimpiarFormulario()
        {
            rutComboBox.SelectedIndex = -1;
            LimpiarDatosCliente();
            ventasDataGridView.Rows.Clear();
            pagarConCreditoCheckBox.Checked = false;
            totalVenta = 0;
            totalVentaTextBox.Text = "0";
            numeroGuiaTextBox.Text = sqliteService.GetUltimoNumeroGuia().ToString();
            fechaEmisionPicker.Value = DateTime.Now;
        }

        private void LimpiarDatosCliente()
        {
            clienteTextBox.Clear();
            direccionTextBox.Clear();
            giroTextBox.Clear();
        }

        private void ImprimirButton_Click(object sender, EventArgs e)
        {
            if (!ValidarVenta()) return;

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
            printPreviewDialog.Document = printDocument;
            printPreviewDialog.ShowDialog();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Font font = new Font("Arial", 12);
            Font boldFont = new Font("Arial", 12, FontStyle.Bold);
            float fontHeight = font.GetHeight();
            int startX = 10;
            int startY = 10;
            int offsetY = 40;

            // Título
            graphics.DrawString("Factura Interna", new Font("Arial", 18, FontStyle.Bold),
                new SolidBrush(Color.Black), startX, startY);
            offsetY += (int)fontHeight + 20;

            // Información del cliente
            graphics.DrawString($"Cliente: {clienteTextBox.Text}", boldFont,
                new SolidBrush(Color.Black), startX, startY + offsetY);
            offsetY += (int)fontHeight + 5;

            graphics.DrawString($"RUT: {rutComboBox.Text}", boldFont,
                new SolidBrush(Color.Black), startX, startY + offsetY);
            offsetY += (int)fontHeight + 5;

            graphics.DrawString($"Dirección: {direccionTextBox.Text}", boldFont,
                new SolidBrush(Color.Black), startX, startY + offsetY);
            offsetY += (int)fontHeight + 5;

            graphics.DrawString($"Giro: {giroTextBox.Text}", boldFont,
                new SolidBrush(Color.Black), startX, startY + offsetY);
            offsetY += (int)fontHeight + 5;

            graphics.DrawString($"Fecha: {fechaEmisionPicker.Value:dd/MM/yyyy}", boldFont,
                new SolidBrush(Color.Black), startX, startY + offsetY);
            offsetY += (int)fontHeight + 20;

            // Línea separadora
            graphics.DrawLine(new Pen(Color.Black), startX, startY + offsetY,
                startX + 800, startY + offsetY);
            offsetY += 10;

            // Encabezados
            string[] headers = { "Código", "Descripción", "Unidades", "Bandejas",
                "Kilos Neto", "Precio", "Total" };
            int[] columnWidths = { 80, 200, 80, 80, 100, 100, 120 };
            int currentX = startX;

            for (int i = 0; i < headers.Length; i++)
            {
                graphics.DrawString(headers[i], boldFont, new SolidBrush(Color.Black),
                    currentX, startY + offsetY);
                currentX += columnWidths[i];
            }
            offsetY += (int)fontHeight + 5;

            // Línea separadora
            graphics.DrawLine(new Pen(Color.Black), startX, startY + offsetY,
                startX + 800, startY + offsetY);
            offsetY += 10;

            // Detalles de la venta
            foreach (DataGridViewRow row in ventasDataGridView.Rows)
            {
                if (row.IsNewRow) continue;

                currentX = startX;
                string[] values = {
                    row.Cells["Codigo"].Value?.ToString(),
                    row.Cells["Descripcion"].Value?.ToString(),
                    row.Cells["Unidades"].Value?.ToString(),
                    row.Cells["Bandejas"].Value?.ToString(),
                    $"{row.Cells["KilosNeto"].Value:N2}",
                    $"${row.Cells["Precio"].Value:N0}",
                    $"${row.Cells["Total"].Value:N0}"
                };

                for (int i = 0; i < values.Length; i++)
                {
                    graphics.DrawString(values[i], font, new SolidBrush(Color.Black),
                        currentX, startY + offsetY);
                    currentX += columnWidths[i];
                }
                offsetY += (int)fontHeight + 5;
            }

            // Línea final
            graphics.DrawLine(new Pen(Color.Black), startX, startY + offsetY,
                startX + 800, startY + offsetY);
            offsetY += 20;

            // Total y forma de pago
            string formaPago = pagarConCreditoCheckBox.Checked ? "CRÉDITO" : "CONTADO";
            graphics.DrawString($"Forma de Pago: {formaPago}", boldFont,
                new SolidBrush(Color.Black), startX, startY + offsetY);
            graphics.DrawString($"Total: ${totalVenta:N0}", boldFont,
                new SolidBrush(Color.Black), startX + 600, startY + offsetY);
        }

        private void ExportarExcelButton_Click(object sender, EventArgs e)
        {
            if (!ValidarVenta()) return;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Venta");

                    // Información del cliente
                    worksheet.Cell("A1").Value = "FACTURA INTERNA";
                    worksheet.Cell("A3").Value = "Cliente:";
                    worksheet.Cell("B3").Value = clienteTextBox.Text;
                    worksheet.Cell("A4").Value = "RUT:";
                    worksheet.Cell("B4").Value = rutComboBox.Text;
                    worksheet.Cell("A5").Value = "Dirección:";
                    worksheet.Cell("B5").Value = direccionTextBox.Text;
                    worksheet.Cell("A6").Value = "Giro:";
                    worksheet.Cell("B6").Value = giroTextBox.Text;
                    worksheet.Cell("A7").Value = "Fecha:";
                    worksheet.Cell("B7").Value = fechaEmisionPicker.Value.ToString("dd/MM/yyyy");

                    // Encabezados
                    int row = 9;
                    worksheet.Cell(row, 1).Value = "Código";
                    worksheet.Cell(row, 2).Value = "Descripción";
                    worksheet.Cell(row, 3).Value = "Unidades";
                    worksheet.Cell(row, 4).Value = "Bandejas";
                    worksheet.Cell(row, 5).Value = "Kilos Neto";
                    worksheet.Cell(row, 6).Value = "Precio";
                    worksheet.Cell(row, 7).Value = "Total";

                    // Datos
                    row++;
                    foreach (DataGridViewRow dgvRow in ventasDataGridView.Rows)
                    {
                        if (dgvRow.IsNewRow) continue;

                        worksheet.Cell(row, 1).Value = dgvRow.Cells["Codigo"].Value?.ToString();
                        worksheet.Cell(row, 2).Value = dgvRow.Cells["Descripcion"].Value?.ToString();
                        worksheet.Cell(row, 3).Value = dgvRow.Cells["Unidades"].Value?.ToString();
                        worksheet.Cell(row, 4).Value = dgvRow.Cells["Bandejas"].Value?.ToString();
                        worksheet.Cell(row, 5).Value = dgvRow.Cells["KilosNeto"].Value?.ToString();
                        worksheet.Cell(row, 6).Value = dgvRow.Cells["Precio"].Value?.ToString();
                        worksheet.Cell(row, 7).Value = dgvRow.Cells["Total"].Value?.ToString();
                        row++;
                    }

                    // Total y forma de pago
                    row += 2;
                    worksheet.Cell(row, 1).Value = "Forma de Pago:";
                    worksheet.Cell(row, 2).Value = pagarConCreditoCheckBox.Checked ? "CRÉDITO" : "CONTADO";
                    worksheet.Cell(row, 6).Value = "Total:";
                    worksheet.Cell(row, 7).Value = totalVenta;

                    // Dar formato
                    var rango = worksheet.Range("A1:G" + row);
                    rango.Style.Font.FontName = "Arial";
                    rango.Style.Font.FontSize = 12;
                    worksheet.Columns().AdjustToContents();

                    // Guardar
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Excel Files|*.xlsx";
                        saveFileDialog.Title = "Guardar Venta";
                        saveFileDialog.FileName = $"Venta_{numeroGuiaTextBox.Text}_{DateTime.Now:yyyyMMdd}.xlsx";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            workbook.SaveAs(saveFileDialog.FileName);
                            MessageBox.Show("Archivo Excel generado exitosamente.",
                                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar a Excel: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

