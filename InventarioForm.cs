using System;
using System.Data.SQLite;
using System.Windows.Forms;
using AdminSERMAC.Models;
using AdminSERMAC.Services;

namespace AdminSERMAC.Forms
{
    public class InventarioForm : Form
    {
        private Label numeroCompraLabel;
        private TextBox numeroCompraTextBox;
        private Label fechaCompraLabel;
        private DateTimePicker fechaCompraPicker;
        private Label proveedorLabel;
        private ComboBox proveedorComboBox;
        private Label vendedorLabel;
        private ComboBox vendedorComboBox;

        private DataGridView inventarioDataGridView;
        private Button agregarButton;
        private Button visualizarInventarioButton;

        private SQLiteService sqliteService;

        public InventarioForm()
        {
            this.Text = "Gestión de Inventario";
            this.Width = 900;
            this.Height = 750;

            sqliteService = new SQLiteService();

            // Número de Compra
            numeroCompraLabel = new Label() { Text = "Número de Compra", Top = 20, Left = 20, Width = 150 };
            numeroCompraTextBox = new TextBox() { Top = 20, Left = 180, Width = 200, ReadOnly = true };
            numeroCompraTextBox.Text = sqliteService.GetUltimoNumeroCompra().ToString();

            // Fecha de Compra
            fechaCompraLabel = new Label() { Text = "Fecha de Compra", Top = 50, Left = 20, Width = 150 };
            fechaCompraPicker = new DateTimePicker() { Top = 50, Left = 180, Width = 200 };

            // Proveedor
            proveedorLabel = new Label() { Text = "Proveedor", Top = 80, Left = 20, Width = 150 };
            proveedorComboBox = new ComboBox() { Top = 80, Left = 180, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            // Vendedor
            vendedorLabel = new Label() { Text = "Vendedor", Top = 110, Left = 20, Width = 150 };
            vendedorComboBox = new ComboBox() { Top = 110, Left = 180, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            // Tabla de Inventario
            inventarioDataGridView = new DataGridView()
            {
                Top = 150,
                Left = 20,
                Width = 850,
                Height = 400,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            inventarioDataGridView.Columns.Add("Codigo", "Código");
            inventarioDataGridView.Columns.Add("Producto", "Producto");
            inventarioDataGridView.Columns.Add("Unidades", "Unidades");
            inventarioDataGridView.Columns.Add("Kilos", "Kilos");
            inventarioDataGridView.Columns.Add("Fecha", "Fecha");

            // Agregar evento para manejar cambios en las celdas
            inventarioDataGridView.CellEndEdit += InventarioDataGridView_CellEndEdit;

            // Botones
            agregarButton = new Button() { Text = "Agregar Productos", Top = 570, Left = 20, Width = 200 };
            agregarButton.Click += AgregarButton_Click;

            visualizarInventarioButton = new Button() { Text = "Visualizar Inventario", Top = 570, Left = 240, Width = 200 };
            visualizarInventarioButton.Click += VisualizarInventarioButton_Click;

            this.Controls.Add(numeroCompraLabel);
            this.Controls.Add(numeroCompraTextBox);
            this.Controls.Add(fechaCompraLabel);
            this.Controls.Add(fechaCompraPicker);
            this.Controls.Add(proveedorLabel);
            this.Controls.Add(proveedorComboBox);
            this.Controls.Add(vendedorLabel);
            this.Controls.Add(vendedorComboBox);
            this.Controls.Add(inventarioDataGridView);
            this.Controls.Add(agregarButton);
            this.Controls.Add(visualizarInventarioButton);

            CargarProveedores();
            CargarVendedores();
        }

        private void CargarProveedores()
        {
            var proveedores = sqliteService.GetProveedores(); // Devuelve nombres de proveedores
            if (proveedores.Count > 0)
            {
                proveedorComboBox.DataSource = proveedores;
            }
            else
            {
                proveedorComboBox.Items.Add("Sin Proveedores");
                MessageBox.Show("No se encontraron proveedores en la base de datos.");
            }
        }

        private void CargarVendedores()
        {
            var vendedores = sqliteService.GetVendedores(); // Devuelve nombres de vendedores
            if (vendedores.Count > 0)
            {
                vendedorComboBox.DataSource = vendedores;
            }
            else
            {
                vendedorComboBox.Items.Add("Sin Vendedores");
                MessageBox.Show("No se encontraron vendedores en la base de datos.");
            }
        }

        private void InventarioDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == inventarioDataGridView.Columns["Codigo"].Index)
            {
                string codigo = inventarioDataGridView.Rows[e.RowIndex].Cells["Codigo"].Value?.ToString();
                if (!string.IsNullOrEmpty(codigo))
                {
                    using (var connection = new SQLiteConnection(sqliteService.connectionString))
                    {
                        connection.Open();
                        var command = new SQLiteCommand(
                            "SELECT Codigo as Codigo, Nombre FROM Productos WHERE Codigo = @codigo", connection);
                        command.Parameters.AddWithValue("@codigo", codigo);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                inventarioDataGridView.Rows[e.RowIndex].Cells["Producto"].Value = reader["Nombre"].ToString();
                            }
                            else
                            {
                                inventarioDataGridView.Rows[e.RowIndex].Cells["Codigo"].Value = null;
                                inventarioDataGridView.Rows[e.RowIndex].Cells["Producto"].Value = null;
                                MessageBox.Show("Código de producto no encontrado", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void AgregarButton_Click(object sender, EventArgs e)
        {
            string numeroCompra = numeroCompraTextBox.Text;
            string fechaCompra = fechaCompraPicker.Value.ToString("yyyy-MM-dd");
            string proveedor = proveedorComboBox.SelectedItem?.ToString();
            string vendedor = vendedorComboBox.SelectedItem?.ToString();

            foreach (DataGridViewRow row in inventarioDataGridView.Rows)
            {
                if (row.IsNewRow) continue;

                string codigo = row.Cells["Codigo"].Value?.ToString();
                string producto = row.Cells["Producto"].Value?.ToString();
                int unidades = int.TryParse(row.Cells["Unidades"].Value?.ToString(), out int u) ? u : 0;
                double kilos = double.TryParse(row.Cells["Kilos"].Value?.ToString(), out double k) ? k : 0.0;
                string fecha = row.Cells["Fecha"].Value?.ToString();

                if (!string.IsNullOrEmpty(codigo))
                {
                    sqliteService.AddProducto(new Producto
                    {
                        Codigo = codigo,
                        Nombre = producto,
                        Unidades = unidades,
                        Kilos = kilos,
                        FechaMasAntigua = fechaCompra,
                        FechaMasNueva = fecha
                    });
                }
            }

            // Limpiar el DataGridView después de agregar el inventario
            inventarioDataGridView.Rows.Clear();

            sqliteService.IncrementarNumeroCompra();
            numeroCompraTextBox.Text = sqliteService.GetUltimoNumeroCompra().ToString();
            MessageBox.Show("Productos actualizados en el inventario.");
        }

        private void VisualizarInventarioButton_Click(object sender, EventArgs e)
        {
            VisualizarInventarioForm visualizarInventarioForm = new VisualizarInventarioForm();
            visualizarInventarioForm.Show();
        }
    }
}











