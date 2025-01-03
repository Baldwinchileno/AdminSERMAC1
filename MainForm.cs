using System;
using System.Windows.Forms;
using AdminSERMAC.Forms;
using AdminSERMAC.Core.Interfaces;
using AdminSERMAC.Services;
using System.Drawing;
using System.IO;

namespace AdminSERMAC
{
    public class MainForm : Form
    {
        private readonly IClienteService _clienteService;
        private readonly string connectionString;

        private Button mostrarClientesButton;
        private Button mostrarVentasButton;
        private Button mostrarInventarioButton;
        private Button reiniciarBaseDatosButton;
        private Button importarProductosButton;

        public MainForm(IClienteService clienteService, string connectionString)
        {
            _clienteService = clienteService;
            this.connectionString = connectionString;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Configuración del formulario
            this.Text = "Menú Principal - SERMAC";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel principal
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            this.Controls.Add(mainPanel);

            // Título
            Label titleLabel = new Label
            {
                Text = "Sistema de Gestión SERMAC",
                Font = new Font("Arial", 20, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 20)
            };
            mainPanel.Controls.Add(titleLabel);

            // Botones principales
            mostrarClientesButton = CreateMenuButton("Gestión de Clientes", 100);
            mostrarVentasButton = CreateMenuButton("Gestión de Ventas", 170);
            mostrarInventarioButton = CreateMenuButton("Gestión de Inventario", 240);
            reiniciarBaseDatosButton = CreateDangerButton("Reiniciar Base de Datos", 430);
            importarProductosButton = CreateDangerButton("Importar Productos", 360);

            // Agregar botones al panel
            mainPanel.Controls.Add(mostrarClientesButton);
            mainPanel.Controls.Add(mostrarVentasButton);
            mainPanel.Controls.Add(mostrarInventarioButton);
            mainPanel.Controls.Add(reiniciarBaseDatosButton);
            mainPanel.Controls.Add(importarProductosButton);

            // Eventos
            mostrarClientesButton.Click += MostrarClientesButton_Click;
            mostrarVentasButton.Click += MostrarVentasButton_Click;
            mostrarInventarioButton.Click += MostrarInventarioButton_Click;
            reiniciarBaseDatosButton.Click += ReiniciarBaseDatosButton_Click;
            importarProductosButton.Click += ImportarProductosButton_Click;
        }

        private Button CreateMenuButton(string text, int top)
        {
            return new Button
            {
                Text = text,
                Top = top,
                Left = 50,
                Width = 200,
                Height = 40,
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private Button CreateDangerButton(string text, int top)
        {
            return new Button
            {
                Text = text,
                Top = top,
                Left = 50,
                Width = 200,
                Height = 40,
                Font = new Font("Arial", 10),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void MostrarClientesButton_Click(object sender, EventArgs e)
        {
            try
            {
                var clientesForm = new ClientesForm(_clienteService);
                clientesForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario de clientes: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarVentasButton_Click(object sender, EventArgs e)
        {
            try
            {
                var ventasForm = new VentasForm();
                ventasForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario de ventas: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarInventarioButton_Click(object sender, EventArgs e)
        {
            try
            {
                var inventarioForm = new InventarioForm();
                inventarioForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario de inventario: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportarProductosButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Archivos CSV (*.csv)|*.csv|Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "Seleccionar archivo de productos";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var resultado = MessageBox.Show(
                        "¿Está seguro que desea importar los productos?\n" +
                        "Se recomienda reiniciar la base de datos antes de importar.",
                        "Confirmar Importación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (resultado == DialogResult.Yes)
                    {
                        try
                        {
                            Cursor = Cursors.WaitCursor;
                            string csvContent = File.ReadAllText(openFileDialog.FileName);
                            var sqliteService = new SQLiteService();
                            sqliteService.ImportarProductosDesdeCSV(csvContent);
                            MessageBox.Show("Productos importados exitosamente", "Éxito",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error al importar productos: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            Cursor = Cursors.Default;
                        }
                    }
                }
            }
        }

        private void ImportarProductosIniciales()
        {
            try
            {
                string csvContent = File.ReadAllText("productos-csv.txt");
                var sqliteService = new SQLiteService();
                sqliteService.ImportarProductosDesdeCSV(csvContent);
                MessageBox.Show("Productos importados exitosamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al importar productos: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ReiniciarBaseDatosButton_Click(object sender, EventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Está seguro que desea reiniciar la base de datos?\n\n" +
                "ADVERTENCIA: Se eliminarán todos los datos existentes.\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar Reinicio",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (resultado == DialogResult.Yes)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    var initializer = new DatabaseInitializer(connectionString);
                    initializer.ReiniciarBaseDeDatos();

                    MessageBox.Show(
                        "Base de datos reiniciada exitosamente.\n" +
                        "Se han creado las tablas necesarias.",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al reiniciar la base de datos:\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }
    }
}