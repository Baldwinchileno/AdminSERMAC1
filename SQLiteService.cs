using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using AdminSERMAC.Models;

namespace AdminSERMAC.Services
{
    public class SQLiteService
    {
        public string connectionString { get; private set; }
        private object rut;

        public SQLiteService()
        {
            connectionString = "Data Source=AdminSERMAC.db;Version=3;";
            EnsureTablesExist();
            EnsureColumnsExist();
        }

        private void EnsureTablesExist()
        {
            EnsureClientesTableExists();
            EnsureConfiguracionTableExists();
            EnsureInventarioTableExists();
            EnsureProveedoresTableExists();
            EnsureVentasTableExists();
        }

        public void EnsureColumnsExist()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);

                // Verificar y agregar columna Deuda a la tabla Clientes si no existe
                if (!ColumnExists(connection, "Clientes", "Deuda"))
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Clientes ADD COLUMN Deuda REAL DEFAULT 0", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Verificar y agregar columna Precio a la tabla Productos si no existe
                if (!ColumnExists(connection, "Productos", "Precio"))
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Productos ADD COLUMN Precio REAL", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Verificar y agregar columna PagadoConCredito a la tabla Ventas si no existe
                if (!ColumnExists(connection, "Ventas", "PagadoConCredito"))
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Ventas ADD COLUMN PagadoConCredito INTEGER DEFAULT 0", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Verificar y agregar columna RUT a la tabla Ventas si no existe
                if (!ColumnExists(connection, "Ventas", "RUT"))
                {
                    using (var command = new SQLiteCommand("ALTER TABLE Ventas ADD COLUMN RUT TEXT", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private bool ColumnExists(SQLiteConnection connection, string tableName, string columnName)
        {
            using (var command = new SQLiteCommand($"PRAGMA table_info({tableName})", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["name"].ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        private void EnableForeignKeys(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public void EnsureClientesTableExists()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Clientes (
                            RUT TEXT PRIMARY KEY,
                            Nombre TEXT,
                            Direccion TEXT,
                            Giro TEXT,
                            Deuda REAL DEFAULT 0
                        )";
                    command.ExecuteNonQuery();
                }
            }
        }

        public void EnsureConfiguracionTableExists()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Configuracion (
                        Clave TEXT PRIMARY KEY,
                        Valor TEXT NOT NULL
                    );";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                string insertDefaultQuery = @"
                    INSERT OR IGNORE INTO Configuracion (Clave, Valor) VALUES ('UltimoNumeroGuia', '0');
                    INSERT OR IGNORE INTO Configuracion (Clave, Valor) VALUES ('UltimoNumeroCompra', '0');";

                using (var command = new SQLiteCommand(insertDefaultQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void EnsureInventarioTableExists()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Inventario (
                        Codigo TEXT PRIMARY KEY,
                        Producto TEXT NOT NULL,
                        Unidades INTEGER NOT NULL,
                        Kilos REAL NOT NULL,
                        FechaMasAntigua TEXT,
                        FechaMasNueva TEXT
                    );";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void EnsureProveedoresTableExists()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Proveedores (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL,
                        Vendedor TEXT NOT NULL
                    );";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void EnsureVentasTableExists()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Ventas (
                        NumeroGuia INTEGER,
                        CodigoProducto TEXT,
                        Descripcion TEXT,
                        Bandejas INTEGER,
                        KilosNeto REAL,
                        FechaVenta TEXT,
                        PagadoConCredito INTEGER DEFAULT 0,
                        RUT TEXT,
                        PRIMARY KEY (NumeroGuia, CodigoProducto),
                        FOREIGN KEY (CodigoProducto) REFERENCES Productos(Codigo),
                        FOREIGN KEY (RUT) REFERENCES Clientes(RUT)
                    );";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetProveedores()
        {
            List<string> proveedores = new List<string>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string query = "SELECT DISTINCT Nombre FROM Proveedores";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            proveedores.Add(reader["Nombre"].ToString());
                        }
                    }
                }
            }

            return proveedores;
        }

        public List<string> GetVendedores()
        {
            List<string> vendedores = new List<string>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string query = "SELECT DISTINCT Vendedor FROM Proveedores";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vendedores.Add(reader["Vendedor"].ToString());
                        }
                    }
                }
            }

            return vendedores;
        }

        public List<Venta> GetVentasPorCliente(string rut)
        {
            var ventas = new List<Venta>();
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                SELECT v.NumeroGuia, v.CodigoProducto, v.Descripcion, v.Bandejas, v.KilosNeto, v.FechaVenta, (v.KilosNeto * p.Precio) AS Total, c.Deuda
                FROM Ventas v
                JOIN Clientes c ON c.RUT = @RUT
                JOIN Productos p ON p.Codigo = v.CodigoProducto
                WHERE c.RUT = @RUT AND v.PagadoConCredito = 1";
                    command.Parameters.AddWithValue("@RUT", rut);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ventas.Add(new Venta
                            {
                                NumeroGuia = Convert.ToInt32(reader["NumeroGuia"]),
                                CodigoProducto = reader["CodigoProducto"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Bandejas = Convert.ToInt32(reader["Bandejas"]),
                                KilosNeto = Convert.ToDouble(reader["KilosNeto"]),
                                FechaVenta = reader["FechaVenta"].ToString(),
                                Total = Convert.ToDouble(reader["Total"]),
                                Deuda = Convert.ToDouble(reader["Deuda"]) // Asegúrate de agregar esta línea
                            });
                        }
                    }
                }
            }
            return ventas;
        }


        public int GetUltimoNumeroGuia()
        {
            int ultimoNumero = 0;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string query = "SELECT Valor FROM Configuracion WHERE Clave = 'UltimoNumeroGuia'";
                using (var command = new SQLiteCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        int.TryParse(result.ToString(), out ultimoNumero);
                    }
                }
            }

            return ultimoNumero;
        }

        public void IncrementarNumeroGuia()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string query = @"
                    UPDATE Configuracion
                    SET Valor = CAST((CAST(Valor AS INTEGER) + 1) AS TEXT)
                    WHERE Clave = 'UltimoNumeroGuia';";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetUltimoNumeroCompra()
        {
            int ultimoNumero = 0;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string query = "SELECT Valor FROM Configuracion WHERE Clave = 'UltimoNumeroCompra'";
                using (var command = new SQLiteCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        int.TryParse(result.ToString(), out ultimoNumero);
                    }
                }
            }

            return ultimoNumero;
        }

        public void ImportarProductosDesdeCSV(string csvContent)
        {
            var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var values = line.Split(',');

                if (values.Length >= 6)
                {
                    string codigo = values[0];
                    string producto = values[1];
                    int unidades = int.Parse(values[2]);
                    double kilos = double.Parse(values[3]);
                    string fechaMasAntigua = values[4];
                    string fechaMasNueva = values[5];

                    AddProducto(codigo, producto, unidades, kilos, fechaMasAntigua, fechaMasNueva);
                }
            }
        }

        // Método AddProducto ya existente en la clase
        public void AddProducto(string codigo, string producto, int unidades, double kilos, string fechaMasAntigua, string fechaMasNueva)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);

                // Verificar si el producto ya existe en el inventario
                string checkQuery = "SELECT COUNT(*) FROM Inventario WHERE Codigo = @codigo";
                using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@codigo", codigo);
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (count > 0)
                    {
                        // Si el producto existe, actualizamos los datos
                        string updateQuery = @"
                        UPDATE Inventario
                        SET 
                            Unidades = Unidades + @unidades,
                            Kilos = Kilos + @kilos,
                            FechaMasAntigua = CASE WHEN FechaMasAntigua > @fechaMasAntigua THEN @fechaMasAntigua ELSE FechaMasAntigua END,
                            FechaMasNueva = CASE WHEN FechaMasNueva < @fechaMasNueva THEN @fechaMasNueva ELSE FechaMasNueva END
                        WHERE Codigo = @codigo";

                        using (var updateCommand = new SQLiteCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@codigo", codigo);
                            updateCommand.Parameters.AddWithValue("@unidades", unidades);
                            updateCommand.Parameters.AddWithValue("@kilos", kilos);
                            updateCommand.Parameters.AddWithValue("@fechaMasAntigua", fechaMasAntigua);
                            updateCommand.Parameters.AddWithValue("@fechaMasNueva", fechaMasNueva);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Si el producto no existe, lo insertamos
                        string insertQuery = @"
                        INSERT INTO Inventario (Codigo, Producto, Unidades, Kilos, FechaMasAntigua, FechaMasNueva)
                        VALUES (@codigo, @producto, @unidades, @kilos, @fechaMasAntigua, @fechaMasNueva)";

                        using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@codigo", codigo);
                            insertCommand.Parameters.AddWithValue("@producto", producto);
                            insertCommand.Parameters.AddWithValue("@unidades", unidades);
                            insertCommand.Parameters.AddWithValue("@kilos", kilos);
                            insertCommand.Parameters.AddWithValue("@fechaMasAntigua", fechaMasAntigua);
                            insertCommand.Parameters.AddWithValue("@fechaMasNueva", fechaMasNueva);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public void IncrementarNumeroCompra()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string query = @"
                    UPDATE Configuracion
                    SET Valor = CAST((CAST(Valor AS INTEGER) + 1) AS TEXT)
                    WHERE Clave = 'UltimoNumeroCompra';";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public Producto GetProductoPorCodigo(string codigo)
        {
            Producto producto = null;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string query = "SELECT * FROM Inventario WHERE Codigo = @codigo";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@codigo", codigo);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            producto = new Producto
                            {
                                Codigo = reader["Codigo"].ToString(),
                                Nombre = reader["Producto"].ToString(),
                                Unidades = int.Parse(reader["Unidades"].ToString()),
                                Kilos = double.Parse(reader["Kilos"].ToString()),
                                FechaMasAntigua = reader["FechaMasAntigua"].ToString(),
                                FechaMasNueva = reader["FechaMasNueva"].ToString()
                            };
                        }
                    }
                }
            }

            return producto;
        }

        public void ActualizarDeudaCliente(string rut, double monto)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "UPDATE Clientes SET Deuda = Deuda + @Monto WHERE RUT = @RUT";
                    command.Parameters.AddWithValue("@Monto", monto);
                    command.Parameters.AddWithValue("@RUT", rut);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DescontarInventario(string codigo, int unidades, double kilos)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string updateQuery = @"
                    UPDATE Inventario
                    SET Unidades = Unidades - @unidades,
                        Kilos = Kilos - @kilos
                    WHERE Codigo = @codigo";

                using (var command = new SQLiteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@codigo", codigo);
                    command.Parameters.AddWithValue("@unidades", unidades);
                    command.Parameters.AddWithValue("@kilos", kilos);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AgregarVenta(int numeroGuia, string codigoProducto, string descripcion, int bandejas, double kilosNeto, string fechaVenta)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string insertQuery = @"
                    INSERT INTO Ventas (NumeroGuia, CodigoProducto, Descripcion, Bandejas, KilosNeto, FechaVenta, PagadoConCredito, RUT)
                    VALUES (@numeroGuia, @codigoProducto, @descripcion, @bandejas, @kilosNeto, @fechaVenta, @pagadoConCredito, @rut)";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@numeroGuia", numeroGuia);
                    command.Parameters.AddWithValue("@codigoProducto", codigoProducto);
                    command.Parameters.AddWithValue("@descripcion", descripcion);
                    command.Parameters.AddWithValue("@bandejas", bandejas);
                    command.Parameters.AddWithValue("@kilosNeto", kilosNeto);
                    command.Parameters.AddWithValue("@fechaVenta", fechaVenta);
                    command.Parameters.AddWithValue("@pagadoConCredito", 1); // Asumimos que siempre se paga con crédito
                    command.Parameters.AddWithValue("@rut", rut);
                    command.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetInventario()
        {
            DataTable inventario = new DataTable();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);

                string query = "SELECT * FROM Inventario";
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(inventario);
                    }
                }
            }

            return inventario;
        }

        public DataTable GetInventarioPorCodigo(string codigo)
        {
            DataTable inventario = new DataTable();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);

                string query = "SELECT * FROM Inventario WHERE Codigo = @codigo";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@codigo", codigo);
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(inventario);
                    }
                }
            }

            return inventario;
        }

        public void ActualizarFechasInventario(string codigo, DateTime fechaIngresada)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string updateQuery = @"
                    UPDATE Inventario
                    SET 
                        FechaMasAntigua = CASE WHEN FechaMasAntigua > @fecha THEN @fecha ELSE FechaMasAntigua END,
                        FechaMasNueva = CASE WHEN FechaMasNueva < @fecha THEN @fecha ELSE FechaMasNueva END
                    WHERE Codigo = @codigo";

                using (var command = new SQLiteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@codigo", codigo);
                    command.Parameters.AddWithValue("@fecha", fechaIngresada.ToString("yyyy-MM-dd"));
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Cliente> GetClientes()
        {
            List<Cliente> clientes = new List<Cliente>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string query = "SELECT * FROM Clientes";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clientes.Add(new Cliente
                            {
                                RUT = reader["RUT"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Direccion = reader["Direccion"].ToString(),
                                Giro = reader["Giro"].ToString(),
                                Deuda = reader["Deuda"] != DBNull.Value ? Convert.ToDouble(reader["Deuda"]) : 0
                            });
                        }
                    }
                }

                return clientes;
            }

        }

        public Cliente GetClientePorRUT(string rut)
        {
            Cliente cliente = null;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);
                string query = "SELECT * FROM Clientes WHERE RUT = @rut";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@rut", rut);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cliente = new Cliente
                            {
                                RUT = reader["RUT"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Direccion = reader["Direccion"].ToString(),
                                Giro = reader["Giro"].ToString(),
                                Deuda = reader["Deuda"] != DBNull.Value ? Convert.ToDouble(reader["Deuda"]) : 0
                            };
                        }
                    }
                }
            }

            return cliente;
        }
    }
}


















