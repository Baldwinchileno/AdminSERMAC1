using System.Data.SQLite;

namespace AdminSERMAC.Services
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ReiniciarBaseDeDatos()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Lógica para reiniciar la base de datos
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;

                        // Primero eliminar tablas que tienen referencias (foreign keys)
                        command.CommandText = "DROP TABLE IF EXISTS Ventas";
                        command.ExecuteNonQuery();

                        command.CommandText = "DROP TABLE IF EXISTS Inventario";
                        command.ExecuteNonQuery();

                        command.CommandText = "DROP TABLE IF EXISTS Productos";
                        command.ExecuteNonQuery();

                        command.CommandText = "DROP TABLE IF EXISTS Clientes";
                        command.ExecuteNonQuery();

                        command.CommandText = "DROP TABLE IF EXISTS Proveedores";
                        command.ExecuteNonQuery();

                        command.CommandText = "DROP TABLE IF EXISTS Configuracion";
                        command.ExecuteNonQuery();

                        // Ahora crear las tablas en el orden correcto
                        command.CommandText = @"
                    CREATE TABLE Clientes (
                        RUT TEXT PRIMARY KEY,
                        Nombre TEXT NOT NULL,
                        Direccion TEXT,
                        Giro TEXT,
                        Deuda REAL DEFAULT 0
                    )";
                        command.ExecuteNonQuery();

                        command.CommandText = @"
                    CREATE TABLE Productos (
                        Codigo TEXT PRIMARY KEY,
                        Nombre TEXT NOT NULL,
                        Precio REAL DEFAULT 0,
                        Marca TEXT,
                        Categoria TEXT,
                        SubCategoria TEXT,
                        UnidadMedida TEXT
                    )";
                        command.ExecuteNonQuery();

                        command.CommandText = @"
                    CREATE TABLE Inventario (
                        Codigo TEXT PRIMARY KEY,
                        Producto TEXT NOT NULL,
                        Marca TEXT,
                        Unidades INTEGER NOT NULL DEFAULT 0,
                        Kilos REAL NOT NULL DEFAULT 0,
                        FechaMasAntigua TEXT,
                        FechaMasNueva TEXT,
                        FOREIGN KEY (Codigo) REFERENCES Productos(Codigo)
                    )";
                        command.ExecuteNonQuery();

                        command.CommandText = @"
                    CREATE TABLE Ventas (
                        NumeroGuia INTEGER,
                        CodigoProducto TEXT,
                        Descripcion TEXT,
                        Bandejas INTEGER,
                        KilosNeto REAL,
                        FechaVenta TEXT,
                        PagadoConCredito INTEGER DEFAULT 0,
                        RUT TEXT,
                        ClienteNombre TEXT,
                        Total REAL DEFAULT 0,
                        PRIMARY KEY (NumeroGuia, CodigoProducto),
                        FOREIGN KEY (CodigoProducto) REFERENCES Productos(Codigo),
                        FOREIGN KEY (RUT) REFERENCES Clientes(RUT)
                    )";
                        command.ExecuteNonQuery();

                        command.CommandText = @"
                    CREATE TABLE Proveedores (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL,
                        Vendedor TEXT NOT NULL
                    )";
                        command.ExecuteNonQuery();

                        command.CommandText = @"
                    CREATE TABLE Configuracion (
                        Clave TEXT PRIMARY KEY,
                        Valor TEXT NOT NULL
                    )";
                        command.ExecuteNonQuery();

                        // Insertar datos de configuración inicial
                        command.CommandText = @"
                    INSERT INTO Configuracion (Clave, Valor) VALUES ('UltimoNumeroGuia', '0');
                    INSERT INTO Configuracion (Clave, Valor) VALUES ('UltimoNumeroCompra', '0')";
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}        