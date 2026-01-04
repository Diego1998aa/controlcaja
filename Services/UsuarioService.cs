using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using SistemaPOS.Data;
using SistemaPOS.Models;

namespace SistemaPOS.Services
{
    public static class UsuarioService
    {
        public static Usuario Autenticar(string usuario, string password)
        {
            string hashPassword = HashPassword(password);
            string query = "SELECT * FROM Usuarios WHERE NombreUsuario = @user AND Password = @pass AND Activo = 1";
            
            var parameters = new[] {
                new SQLiteParameter("@user", usuario),
                new SQLiteParameter("@pass", hashPassword)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                Usuario u = new Usuario
                {
                    IdUsuario = Convert.ToInt32(row["IdUsuario"]),
                    NombreUsuario = row["NombreUsuario"].ToString(),
                    NombreCompleto = row["NombreCompleto"].ToString(),
                    Rol = row["Rol"].ToString(),
                    Activo = Convert.ToBoolean(row["Activo"])
                };

                // Actualizar último acceso
                string update = "UPDATE Usuarios SET UltimoAcceso = CURRENT_TIMESTAMP WHERE IdUsuario = @id";
                DatabaseHelper.ExecuteQuery(update, new[] { new SQLiteParameter("@id", u.IdUsuario) });

                return u;
            }
            return null;
        }

        public static List<Usuario> ObtenerTodosLosUsuarios()
        {
            List<Usuario> lista = new List<Usuario>();
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Usuarios");
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Usuario
                {
                    IdUsuario = Convert.ToInt32(row["IdUsuario"]),
                    NombreUsuario = row["NombreUsuario"].ToString(),
                    NombreCompleto = row["NombreCompleto"].ToString(),
                    Rol = row["Rol"].ToString(),
                    Activo = Convert.ToBoolean(row["Activo"]),
                    FechaCreacion = Convert.ToDateTime(row["FechaCreacion"]),
                    UltimoAcceso = row["UltimoAcceso"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["UltimoAcceso"]) : null
                });
            }
            return lista;
        }

        public static bool CrearUsuario(string usuario, string password, string nombre, string rol)
        {
            try
            {
                string sql = "INSERT INTO Usuarios (NombreUsuario, Password, NombreCompleto, Rol) VALUES (@user, @pass, @nombre, @rol)";
                DatabaseHelper.ExecuteQuery(sql, new[] {
                    new SQLiteParameter("@user", usuario),
                    new SQLiteParameter("@pass", HashPassword(password)),
                    new SQLiteParameter("@nombre", nombre),
                    new SQLiteParameter("@rol", rol)
                });
                return true;
            }
            catch { return false; }
        }

        public static bool ActualizarUsuario(int id, string nombre, string rol, bool activo)
        {
            string sql = "UPDATE Usuarios SET NombreCompleto = @nombre, Rol = @rol, Activo = @activo WHERE IdUsuario = @id";
            DatabaseHelper.ExecuteQuery(sql, new[] {
                new SQLiteParameter("@nombre", nombre),
                new SQLiteParameter("@rol", rol),
                new SQLiteParameter("@activo", activo ? 1 : 0),
                new SQLiteParameter("@id", id)
            });
            return true;
        }

        public static bool ResetearContraseña(int id, string nuevaPassword)
        {
            string sql = "UPDATE Usuarios SET Password = @pass WHERE IdUsuario = @id";
            DatabaseHelper.ExecuteQuery(sql, new[] {
                new SQLiteParameter("@pass", HashPassword(nuevaPassword)),
                new SQLiteParameter("@id", id)
            });
            return true;
        }

        public static bool EliminarUsuario(int id) => ActualizarUsuario(id, "", "", false); // Soft delete

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }
    }
}