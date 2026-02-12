using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace SistemaPOS.Data
{
    public static class DatabaseHelper
    {
        private static string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pos_database.db");
        private static string connectionString = string.Format("Data Source={0};Version=3;", dbPath);

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            string dir = System.IO.Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (!File.Exists(dbPath))
            {
                string legacyPath = "pos_database.db";
                if (File.Exists(legacyPath))
                {
                    File.Copy(legacyPath, dbPath, true);
                }
                else
                {
                    SQLiteConnection.CreateFile(dbPath);
                    using (var conn = GetConnection())
                    {
                        conn.Open();
                        string sql = @"
                            CREATE TABLE IF NOT EXISTS Usuarios (
                                IdUsuario INTEGER PRIMARY KEY AUTOINCREMENT,
                                NombreUsuario TEXT UNIQUE,
                                Password TEXT,
                                NombreCompleto TEXT,
                                Rol TEXT,
                                Activo INTEGER DEFAULT 1,
                                FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
                                UltimoAcceso DATETIME,
                                Salt TEXT,
                                PasswordHash TEXT
                            );
                            
                            CREATE TABLE IF NOT EXISTS Categorias (
                                IdCategoria INTEGER PRIMARY KEY AUTOINCREMENT,
                                NombreCategoria TEXT UNIQUE
                            );

                            CREATE TABLE IF NOT EXISTS Productos (
                                IdProducto INTEGER PRIMARY KEY AUTOINCREMENT,
                                CodigoBarras TEXT UNIQUE,
                                SKU TEXT,
                                Nombre TEXT NOT NULL,
                                Descripcion TEXT,
                                PrecioCompra DECIMAL(10,2),
                                PrecioVenta DECIMAL(10,2),
                                StockActual INTEGER DEFAULT 0,
                                StockMinimo INTEGER DEFAULT 5,
                                IdCategoria INTEGER,
                                Estado TEXT DEFAULT 'Activo',
                                FOREIGN KEY(IdCategoria) REFERENCES Categorias(IdCategoria)
                            );

                            CREATE TABLE IF NOT EXISTS Ventas (
                                IdVenta INTEGER PRIMARY KEY AUTOINCREMENT,
                                IdPedido INTEGER,
                                IdUsuarioCajero INTEGER,
                                FechaHora DATETIME DEFAULT CURRENT_TIMESTAMP,
                                Subtotal DECIMAL(10,2),
                                IVA DECIMAL(10,2),
                                Descuento DECIMAL(10,2) DEFAULT 0,
                                Total DECIMAL(10,2),
                                MetodoPago TEXT,
                                MontoRecibido DECIMAL(10,2),
                                MontoCambio DECIMAL(10,2),
                                Estado TEXT DEFAULT 'Completada',
                                FOREIGN KEY(IdUsuarioCajero) REFERENCES Usuarios(IdUsuario)
                            );

                            CREATE TABLE IF NOT EXISTS Detalle_Venta (
                                IdDetalle INTEGER PRIMARY KEY AUTOINCREMENT,
                                IdVenta INTEGER,
                                IdProducto INTEGER,
                                Cantidad INTEGER,
                                PrecioUnitario DECIMAL(10,2),
                                Subtotal DECIMAL(10,2),
                                FOREIGN KEY(IdVenta) REFERENCES Ventas(IdVenta),
                                FOREIGN KEY(IdProducto) REFERENCES Productos(IdProducto)
                            );

                            CREATE TABLE IF NOT EXISTS Pedidos (
                                IdPedido INTEGER PRIMARY KEY AUTOINCREMENT,
                                IdUsuarioVendedor INTEGER,
                                IdCliente INTEGER,
                                Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
                                Total DECIMAL(10,2),
                                Estado TEXT DEFAULT 'PENDIENTE',
                                FOREIGN KEY(IdUsuarioVendedor) REFERENCES Usuarios(IdUsuario)
                            );

                            CREATE TABLE IF NOT EXISTS Detalle_Pedido (
                                IdDetallePedido INTEGER PRIMARY KEY AUTOINCREMENT,
                                IdPedido INTEGER,
                                IdProducto INTEGER,
                                Cantidad INTEGER,
                                PrecioUnitario DECIMAL(10,2),
                                Subtotal DECIMAL(10,2),
                                FOREIGN KEY(IdPedido) REFERENCES Pedidos(IdPedido),
                                FOREIGN KEY(IdProducto) REFERENCES Productos(IdProducto)
                            );

                            CREATE TABLE IF NOT EXISTS Movimientos_Inventario (
                                IdMovimiento INTEGER PRIMARY KEY AUTOINCREMENT,
                                IdProducto INTEGER,
                                TipoMovimiento TEXT,
                                Cantidad INTEGER,
                                StockAnterior INTEGER,
                                StockNuevo INTEGER,
                                Motivo TEXT,
                                IdUsuario INTEGER,
                                FechaHora DATETIME DEFAULT CURRENT_TIMESTAMP,
                                FOREIGN KEY(IdProducto) REFERENCES Productos(IdProducto),
                                FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario)
                            );

                            INSERT OR IGNORE INTO Usuarios (NombreUsuario, Password, NombreCompleto, Rol) 
                            VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrador', 'Supervisor');
                            
                            INSERT OR IGNORE INTO Categorias (NombreCategoria) VALUES ('General');
                            INSERT OR IGNORE INTO Categorias (NombreCategoria) VALUES ('Bebidas');
                            INSERT OR IGNORE INTO Categorias (NombreCategoria) VALUES ('Alimentos');
                            INSERT OR IGNORE INTO Categorias (NombreCategoria) VALUES ('Limpieza');
                        ";
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            EnsureMigrations();
        }

        public static DataTable ExecuteQuery(string query, SQLiteParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static int ExecuteNonQuery(string query, SQLiteParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        private static void EnsureMigrations()
        {
            try
            {
                DataTable ucols = ExecuteQuery("PRAGMA table_info(Usuarios)");
                bool hasSalt = false, hasHash = false;
                foreach (DataRow r in ucols.Rows)
                {
                    var name = r["name"].ToString();
                    if (name == "Salt") hasSalt = true;
                    if (name == "PasswordHash") hasHash = true;
                }
                if (!hasSalt) ExecuteNonQuery("ALTER TABLE Usuarios ADD COLUMN Salt TEXT");
                if (!hasHash) ExecuteNonQuery("ALTER TABLE Usuarios ADD COLUMN PasswordHash TEXT");

                DataTable pcols = ExecuteQuery("PRAGMA table_info(Productos)");
                bool hasEstado = false;
                foreach (DataRow r in pcols.Rows)
                {
                    var name = r["name"].ToString();
                    if (name == "Estado") hasEstado = true;
                }
                if (!hasEstado)
                {
                    ExecuteNonQuery("ALTER TABLE Productos ADD COLUMN Estado TEXT DEFAULT 'Activo'");
                    ExecuteNonQuery("UPDATE Productos SET Estado = 'Activo' WHERE Estado IS NULL");
                }

                DataTable vcols = ExecuteQuery("PRAGMA table_info(Ventas)");
                bool hasNumeroVoucher = false;
                bool hasIdPedido = false;
                foreach (DataRow r in vcols.Rows)
                {
                    var name = r["name"].ToString();
                    if (name == "NumeroVoucher") hasNumeroVoucher = true;
                    if (name == "IdPedido") hasIdPedido = true;
                }
                if (!hasNumeroVoucher)
                {
                    ExecuteNonQuery("ALTER TABLE Ventas ADD COLUMN NumeroVoucher TEXT");
                }
                if (!hasIdPedido)
                {
                    ExecuteNonQuery("ALTER TABLE Ventas ADD COLUMN IdPedido INTEGER");
                }

                // Migración para tablas de Pedidos
                ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS Pedidos (
                        IdPedido INTEGER PRIMARY KEY AUTOINCREMENT,
                        IdUsuarioVendedor INTEGER,
                        IdCliente INTEGER,
                        Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
                        Total DECIMAL(10,2),
                        Estado TEXT DEFAULT 'PENDIENTE',
                        FOREIGN KEY(IdUsuarioVendedor) REFERENCES Usuarios(IdUsuario)
                    );

                    CREATE TABLE IF NOT EXISTS Detalle_Pedido (
                        IdDetallePedido INTEGER PRIMARY KEY AUTOINCREMENT,
                        IdPedido INTEGER,
                        IdProducto INTEGER,
                        Cantidad INTEGER,
                        PrecioUnitario DECIMAL(10,2),
                        Subtotal DECIMAL(10,2),
                        FOREIGN KEY(IdPedido) REFERENCES Pedidos(IdPedido),
                        FOREIGN KEY(IdProducto) REFERENCES Productos(IdProducto)
                    );
                ");

                // Migración para Tabla de Configuración
                ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS Configuracion (
                        Clave TEXT PRIMARY KEY,
                        Valor TEXT
                    )
                ");

                // Datos por defecto si no existen
                var count = ExecuteScalar("SELECT COUNT(*) FROM Configuracion");
                if (Convert.ToInt32(count) == 0)
                {
                    ExecuteNonQuery("INSERT INTO Configuracion (Clave, Valor) VALUES ('NombreEmpresa', 'Mi Empresa S.A.')");
                    ExecuteNonQuery("INSERT INTO Configuracion (Clave, Valor) VALUES ('RUT', '11.111.111-1')");
                    ExecuteNonQuery("INSERT INTO Configuracion (Clave, Valor) VALUES ('Direccion', 'Calle Principal #123, Ciudad')");
                    ExecuteNonQuery("INSERT INTO Configuracion (Clave, Valor) VALUES ('Telefono', '+56 9 1234 5678')");
                    ExecuteNonQuery("INSERT INTO Configuracion (Clave, Valor) VALUES ('Giro', 'Venta al por menor')");
                }

                // Migración de roles antiguos a nuevos (Cajero -> Cajera)
                var usuariosConRolAntiguo = ExecuteQuery("SELECT IdUsuario, Rol FROM Usuarios WHERE Rol = 'Cajero'");
                if (usuariosConRolAntiguo.Rows.Count > 0)
                {
                    // Por defecto, convertir "Cajero" antiguo a "Cajera" nuevo
                    // El administrador puede ajustar luego si algunos deben ser "Vendedor"
                    foreach (DataRow row in usuariosConRolAntiguo.Rows)
                    {
                        int id = Convert.ToInt32(row["IdUsuario"]);
                        ExecuteNonQuery("UPDATE Usuarios SET Rol = 'Cajera' WHERE IdUsuario = @id",
                            new SQLiteParameter[] { new SQLiteParameter("@id", id) });
                    }

                    // Mostrar notificación al usuario (solo una vez)
                    var migracionRealizada = ExecuteScalar("SELECT Valor FROM Configuracion WHERE Clave = 'MigracionRolesRealizada'");
                    if (migracionRealizada == null)
                    {
                        ExecuteNonQuery("INSERT INTO Configuracion (Clave, Valor) VALUES ('MigracionRolesRealizada', 'Si')");
                        System.Windows.Forms.MessageBox.Show(
                            "Se han actualizado los roles de usuario.\n" +
                            "Los usuarios 'Cajero' ahora son 'Cajera'.\n" +
                            "Los nuevos roles disponibles son: Vendedor, Cajera, Supervisor.\n" +
                            "Puede ajustar los roles desde el módulo de Usuarios.",
                            "Migración de Roles",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Information
                        );
                    }
                }
            }
            catch { }
        }

        public static string GetConfiguracion(string clave)
        {
            try
            {
                var result = ExecuteScalar("SELECT Valor FROM Configuracion WHERE Clave = @clave", 
                    new SQLiteParameter[] { new SQLiteParameter("@clave", clave) });
                return result != null ? result.ToString() : "";
            }
            catch { return ""; }
        }

        public static void SetConfiguracion(string clave, string valor)
        {
            string sql = "INSERT OR REPLACE INTO Configuracion (Clave, Valor) VALUES (@clave, @valor)";
            ExecuteNonQuery(sql, new SQLiteParameter[] { 
                new SQLiteParameter("@clave", clave),
                new SQLiteParameter("@valor", valor)
            });
        }

        public static object ExecuteScalar(string query, SQLiteParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static void BackupDatabase(string destinationPath)
        {
            File.Copy(dbPath, destinationPath, true);
        }
    }
}
