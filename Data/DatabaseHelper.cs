using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace SistemaPOS.Data
{
    public static class DatabaseHelper
    {
        private static string dbPath = "pos_database.db";
        private static string connectionString = $"Data Source={dbPath};Version=3;";

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
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
                            UltimoAcceso DATETIME
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
                            FOREIGN KEY(IdCategoria) REFERENCES Categorias(IdCategoria)
                        );

                        CREATE TABLE IF NOT EXISTS Ventas (
                            IdVenta INTEGER PRIMARY KEY AUTOINCREMENT,
                            IdUsuarioCajero INTEGER,
                            FechaHora DATETIME DEFAULT CURRENT_TIMESTAMP,
                            Subtotal DECIMAL(10,2),
                            IVA DECIMAL(10,2),
                            Total DECIMAL(10,2),
                            MetodoPago TEXT,
                            MontoRecibido DECIMAL(10,2),
                            MontoCambio DECIMAL(10,2),
                            Estado TEXT DEFAULT 'Completada'
                        );

                        INSERT OR IGNORE INTO Usuarios (NombreUsuario, Password, NombreCompleto, Rol) 
                        VALUES ('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'Administrador', 'Supervisor');
                    ";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
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

        public static void BackupDatabase(string destinationPath) => File.Copy(dbPath, destinationPath, true);
    }
}