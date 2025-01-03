using System;
using System.Data;
using System.Windows.Forms;
using AdminSERMAC.Services;
using System.Data.SQLite;
using System.Drawing;

namespace AdminSERMAC.Forms
{
    public class VisualizarInventarioForm : Form
    {
        private TextBox codigoProductoTextBox;
        private Button buscarButton;
        private Button limpiarFiltroButton;
        private DataGridView inventarioDataGridView;
        private SQLiteService sqliteService;

        public VisualizarInventarioForm()
        {
            this.Text = "Visualizar Inventario";
            this.Width = 1000;
            this.Height = 600;
            sqliteService = new SQLiteService();
            InitializeComponents();
            LoadInventarioData();
        }

        private void InitializeComponents()
        {
            // Panel de búsqueda
            Panel searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            Label codigoProductoLabel = new Label
            {
                Text = "Código del Producto:",
                AutoSize = true,
                Location = new Point(20, 25)
            };

            codigoProductoTextBox = new TextBox
            {
                Location = new Point(150, 22),
                Width = 200
            };

            buscarButton = new Button
            {
                Text = "Buscar",
                Location = new Point(360, 20),
                Width = 100,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };

            limpiarFiltroButton = new Button
            {
                Text = "Limpiar Filtro",
                Location = new Point(470, 20),
                Width = 100
            };

            searchPanel.Controls.AddRange(new Control[]
            {
               codigoProductoLabel,
               codigoProductoTextBox,
               buscarButton,
               limpiarFiltroButton
            });

            // DataGridView
            inventarioDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                MultiSelect = false
            };

            inventarioDataGridView.DefaultCellStyle.Padding = new Padding(5);
            inventarioDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            inventarioDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font(inventarioDataGridView.Font, FontStyle.Bold);
            inventarioDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            // Agregar controles al formulario
            this.Controls.Add(inventarioDataGridView);
            this.Controls.Add(searchPanel);

            // Eventos
            buscarButton.Click += BuscarButton_Click;
            limpiarFiltroButton.Click += LimpiarFiltroButton_Click;
        }

        private void LoadInventarioData(string codigoProducto = null)
        {
            try
            {
                using (var connection = new SQLiteConnection(sqliteService.connectionString))
                {
                    connection.Open();
                    string query = @"
                       SELECT 
                           i.Codigo as 'Código',
                           i.Producto as 'Producto',
                           i.Unidades as 'Unidades',
                           i.Kilos as 'Kilos',
                           i.FechaMasAntigua as 'Fecha Más Antigua',
                           i.FechaMasNueva as 'Fecha Más Nueva'
                       FROM Inventario i";

                    if (!string.IsNullOrEmpty(codigoProducto))
                    {
                        query += " WHERE i.Codigo = @codigo";
                    }

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(codigoProducto))
                        {
                            command.Parameters.AddWithValue("@codigo", codigoProducto);
                        }

                        using (var adapter = new SQLiteDataAdapter(command))
                        {
                            var dt = new DataTable();
                            adapter.Fill(dt);
                            inventarioDataGridView.DataSource = dt;
                        }
                    }
                }

                FormatearGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar inventario: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatearGrid()
        {
            // Formato para columnas numéricas
            if (inventarioDataGridView.Columns["Kilos"] != null)
            {
                inventarioDataGridView.Columns["Kilos"].DefaultCellStyle.Format = "N2";
            }

            // Formato para fechas
            if (inventarioDataGridView.Columns["Fecha Más Antigua"] != null)
            {
                inventarioDataGridView.Columns["Fecha Más Antigua"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (inventarioDataGridView.Columns["Fecha Más Nueva"] != null)
            {
                inventarioDataGridView.Columns["Fecha Más Nueva"].DefaultCellStyle.Format = "dd/MM/yyyy";
            }
        }

        private void BuscarButton_Click(object sender, EventArgs e)
        {
            string codigoProducto = codigoProductoTextBox.Text.Trim();
            LoadInventarioData(codigoProducto);
        }

        private void LimpiarFiltroButton_Click(object sender, EventArgs e)
        {
            codigoProductoTextBox.Clear();
            LoadInventarioData();
        }

        public void RefrescarDatosInventario()
        {
            LoadInventarioData(codigoProductoTextBox.Text.Trim());
        }
    }
}