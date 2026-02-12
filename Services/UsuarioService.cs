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
            string query = "SELECT * FROM Usuarios WHERE NombreUsuario = @user AND Activo = 1";
            var parameters = new[] { new SQLiteParameter("@user", usuario) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                bool hasPBKDF2 = dt.Columns.Contains("Salt") && dt.Columns.Contains("PasswordHash") &&
                                 row["Salt"] != DBNull.Value && row["PasswordHash"] != DBNull.Value &&
                                 !string.IsNullOrEmpty(row["Salt"].ToString()) &&
                                 !string.IsNullOrEmpty(row["PasswordHash"].ToString());

                bool ok = false;
                if (hasPBKDF2)
                {
                    ok = VerifyPBKDF2(password, row["Salt"].ToString(), row["PasswordHash"].ToString());
                }
                else
                {
                    string legacy = HashPasswordSha256(password);
                    ok = dt.Columns.Contains("Password") && row["Password"] != DBNull.Value && row["Password"].ToString() == legacy;
                }

                if (!ok) return null;

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
                DatabaseHelper.ExecuteNonQuery(update, new[] { new SQLiteParameter("@id", u.IdUsuario) });

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
            // VALIDACIÓN DE PERMISOS
            if (!SesionActual.TienePermiso(Permiso.CrearUsuarios))
            {
                throw new UnauthorizedAccessException("No tiene permisos para crear usuarios");
            }

            // Validar que el rol sea válido
            if (!RolesPermisos.ObtenerRolesDisponibles().Contains(rol))
            {
                throw new ArgumentException(string.Format("Rol inválido: {0}", rol));
            }

            try
            {
                string salt = GenerateSalt();
                string hash = HashPBKDF2(password, salt);
                string sql = "INSERT INTO Usuarios (NombreUsuario, Password, NombreCompleto, Rol, Salt, PasswordHash) VALUES (@user, NULL, @nombre, @rol, @salt, @hash)";
                DatabaseHelper.ExecuteNonQuery(sql, new[] {
                    new SQLiteParameter("@user", usuario),
                    new SQLiteParameter("@nombre", nombre),
                    new SQLiteParameter("@rol", rol),
                    new SQLiteParameter("@salt", salt),
                    new SQLiteParameter("@hash", hash)
                });
                return true;
            }
            catch { return false; }
        }

        public static bool ActualizarUsuario(int id, string nombre, string rol, bool activo)
        {
            // VALIDACIÓN DE PERMISOS
            if (!SesionActual.TienePermiso(Permiso.EditarUsuarios))
            {
                throw new UnauthorizedAccessException("No tiene permisos para editar usuarios");
            }

            // No permitir editar usuarios con rol superior
            var usuarios = ObtenerTodosLosUsuarios();
            var usuarioAEditar = usuarios.Find(u => u.IdUsuario == id);
            if (usuarioAEditar != null && usuarioAEditar.Rol == RolesPermisos.Supervisor && !SesionActual.EsSupervisor())
            {
                throw new UnauthorizedAccessException("No puede editar un supervisor");
            }

            // Validar que el rol sea válido
            if (!string.IsNullOrEmpty(rol) && !RolesPermisos.ObtenerRolesDisponibles().Contains(rol))
            {
                throw new ArgumentException(string.Format("Rol inválido: {0}", rol));
            }

            string sql = "UPDATE Usuarios SET NombreCompleto = @nombre, Rol = @rol, Activo = @activo WHERE IdUsuario = @id";
            DatabaseHelper.ExecuteNonQuery(sql, new[] {
                new SQLiteParameter("@nombre", nombre),
                new SQLiteParameter("@rol", rol),
                new SQLiteParameter("@activo", activo ? 1 : 0),
                new SQLiteParameter("@id", id)
            });
            return true;
        }

        public static bool ResetearContraseña(int id, string nuevaPassword)
        {
            // VALIDACIÓN DE PERMISOS
            bool esPropiaContraseña = SesionActual.UsuarioActivo != null && SesionActual.UsuarioActivo.IdUsuario == id;

            if (esPropiaContraseña)
            {
                if (!SesionActual.TienePermiso(Permiso.CambiarPropiaContraseña))
                {
                    throw new UnauthorizedAccessException("No tiene permisos para cambiar contraseñas");
                }
            }
            else
            {
                if (!SesionActual.TienePermiso(Permiso.CambiarContraseñas))
                {
                    throw new UnauthorizedAccessException("No tiene permisos para cambiar contraseñas de otros usuarios");
                }
            }

            string salt = GenerateSalt();
            string hash = HashPBKDF2(nuevaPassword, salt);
            string sql = "UPDATE Usuarios SET Password = NULL, Salt = @salt, PasswordHash = @hash WHERE IdUsuario = @id";
            DatabaseHelper.ExecuteNonQuery(sql, new[] {
                new SQLiteParameter("@salt", salt),
                new SQLiteParameter("@hash", hash),
                new SQLiteParameter("@id", id)
            });
            return true;
        }

        public static bool EliminarUsuario(int id)
        {
            return ActualizarUsuario(id, "", "", false); // Soft delete
        }

        private static string HashPasswordSha256(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        private static string GenerateSalt(int size = 16)
        {
            byte[] salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        private static string HashPBKDF2(string password, string saltBase64, int iterations = 10000, int length = 32)
        {
            byte[] salt = Convert.FromBase64String(saltBase64);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return Convert.ToBase64String(pbkdf2.GetBytes(length));
            }
        }

        private static bool VerifyPBKDF2(string password, string saltBase64, string expectedHashBase64)
        {
            string hash = HashPBKDF2(password, saltBase64);
            return hash == expectedHashBase64;
        }
    }
}
