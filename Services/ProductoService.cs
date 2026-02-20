using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using SistemaPOS.Data;
using SistemaPOS.Models;

namespace SistemaPOS.Services
{
    public static class ProductoService
    {
        public static Producto BuscarPorCodigo(string codigo)
        {
            string query = @"SELECT p.*, c.NombreCategoria FROM Productos p
                             LEFT JOIN Categorias c ON p.IdCategoria = c.IdCategoria
                             WHERE p.CodigoBarras = @codigo OR (p.SKU != '' AND p.SKU = @codigo)";
            DataTable dt = DatabaseHelper.ExecuteQuery(query, new[] { new SQLiteParameter("@codigo", codigo) });

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Producto
                {
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    CodigoBarras = row["CodigoBarras"].ToString(),
                    SKU = row["SKU"].ToString(),
                    Nombre = row["Nombre"].ToString(),
                    Descripcion = row["Descripcion"].ToString(),
                    PrecioCompra = Convert.ToDecimal(row["PrecioCompra"]),
                    PrecioVenta = Convert.ToDecimal(row["PrecioVenta"]),
                    StockActual = Convert.ToInt32(row["StockActual"]),
                    StockMinimo = Convert.ToInt32(row["StockMinimo"]),
                    IdCategoria = row["IdCategoria"] != DBNull.Value ? Convert.ToInt32(row["IdCategoria"]) : 0,
                    NombreCategoria = row["NombreCategoria"] != DBNull.Value ? row["NombreCategoria"].ToString() : "",
                    Estado = row["Estado"] != DBNull.Value ? row["Estado"].ToString() : "Activo"
                };
            }
            return null;
        }

        public static List<Producto> ObtenerTodos(bool soloActivos = true)
        {
            List<Producto> lista = new List<Producto>();
            string sql = "SELECT p.*, c.NombreCategoria FROM Productos p LEFT JOIN Categorias c ON p.IdCategoria = c.IdCategoria";

            if (soloActivos)
                sql += " WHERE (p.Estado = 'Activo' OR p.Estado IS NULL)";

            sql += " ORDER BY p.Nombre ASC";

            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Producto
                {
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    CodigoBarras = row["CodigoBarras"] != DBNull.Value ? row["CodigoBarras"].ToString() : "",
                    SKU = row["SKU"] != DBNull.Value ? row["SKU"].ToString() : "",
                    Nombre = row["Nombre"].ToString(),
                    Descripcion = row["Descripcion"] != DBNull.Value ? row["Descripcion"].ToString() : "",
                    PrecioCompra = Convert.ToDecimal(row["PrecioCompra"]),
                    PrecioVenta = Convert.ToDecimal(row["PrecioVenta"]),
                    StockActual = Convert.ToInt32(row["StockActual"]),
                    StockMinimo = Convert.ToInt32(row["StockMinimo"]),
                    IdCategoria = row["IdCategoria"] != DBNull.Value ? Convert.ToInt32(row["IdCategoria"]) : 0,
                    NombreCategoria = row["NombreCategoria"] != DBNull.Value ? row["NombreCategoria"].ToString() : "",
                    Estado = dt.Columns.Contains("Estado") && row["Estado"] != DBNull.Value ? row["Estado"].ToString() : "Activo"
                });
            }
            return lista;
        }

        public static List<Producto> ObtenerConBajoStock()
        {
            var todos = ObtenerTodos(false);
            return todos.Where(p => p.TieneBajoStock() || p.SinStock()).ToList();
        }

        public static List<Categoria> ObtenerCategorias()
        {
            List<Categoria> lista = new List<Categoria>();
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Categorias ORDER BY NombreCategoria");
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Categoria
                {
                    IdCategoria = Convert.ToInt32(row["IdCategoria"]),
                    NombreCategoria = row["NombreCategoria"].ToString()
                });
            }
            return lista;
        }

        public static bool CrearProducto(Producto p)
        {
            if (!SesionActual.TienePermiso(Permiso.EditarProductos))
                throw new UnauthorizedAccessException("No tiene permisos para crear productos");

            if (ExisteCodigoBarras(p.CodigoBarras))
                throw new InvalidOperationException(string.Format("El código de barras '{0}' ya existe", p.CodigoBarras));

            if (!string.IsNullOrEmpty(p.SKU) && ExisteSKU(p.SKU))
                throw new InvalidOperationException(string.Format("El SKU '{0}' ya existe", p.SKU));

            try
            {
                string sql = @"INSERT INTO Productos (CodigoBarras, SKU, Nombre, Descripcion, PrecioCompra, PrecioVenta, StockActual, StockMinimo, IdCategoria)
                               VALUES (@cod, @sku, @nom, @desc, @pcompra, @pventa, @stock, @min, @cat)";
                DatabaseHelper.ExecuteNonQuery(sql, new[] {
                    new SQLiteParameter("@cod", p.CodigoBarras),
                    new SQLiteParameter("@sku", p.SKU ?? ""),
                    new SQLiteParameter("@nom", p.Nombre),
                    new SQLiteParameter("@desc", p.Descripcion ?? ""),
                    new SQLiteParameter("@pcompra", p.PrecioCompra),
                    new SQLiteParameter("@pventa", p.PrecioVenta),
                    new SQLiteParameter("@stock", p.StockActual),
                    new SQLiteParameter("@min", p.StockMinimo),
                    new SQLiteParameter("@cat", p.IdCategoria)
                });
                return true;
            }
            catch { return false; }
        }

        public static bool ActualizarProducto(Producto p)
        {
            if (!SesionActual.TienePermiso(Permiso.EditarProductos))
                throw new UnauthorizedAccessException("No tiene permisos para editar productos");

            if (ExisteCodigoBarras(p.CodigoBarras, p.IdProducto))
                throw new InvalidOperationException(string.Format("El código de barras '{0}' ya existe en otro producto", p.CodigoBarras));

            if (!string.IsNullOrEmpty(p.SKU) && ExisteSKU(p.SKU, p.IdProducto))
                throw new InvalidOperationException(string.Format("El SKU '{0}' ya existe en otro producto", p.SKU));

            try
            {
                string sql = @"UPDATE Productos SET CodigoBarras=@cod, SKU=@sku, Nombre=@nom, Descripcion=@desc,
                               PrecioCompra=@pcompra, PrecioVenta=@pventa, StockActual=@stock, StockMinimo=@min, IdCategoria=@cat
                               WHERE IdProducto=@id";
                DatabaseHelper.ExecuteNonQuery(sql, new[] {
                    new SQLiteParameter("@cod", p.CodigoBarras),
                    new SQLiteParameter("@sku", p.SKU ?? ""),
                    new SQLiteParameter("@nom", p.Nombre),
                    new SQLiteParameter("@desc", p.Descripcion ?? ""),
                    new SQLiteParameter("@pcompra", p.PrecioCompra),
                    new SQLiteParameter("@pventa", p.PrecioVenta),
                    new SQLiteParameter("@stock", p.StockActual),
                    new SQLiteParameter("@min", p.StockMinimo),
                    new SQLiteParameter("@cat", p.IdCategoria),
                    new SQLiteParameter("@id", p.IdProducto)
                });
                return true;
            }
            catch { return false; }
        }

        public static bool DesactivarProducto(int id)
        {
            if (!SesionActual.TienePermiso(Permiso.EditarProductos))
                throw new UnauthorizedAccessException("No tiene permisos para eliminar productos");

            try
            {
                DatabaseHelper.ExecuteNonQuery("UPDATE Productos SET Estado = 'Inactivo' WHERE IdProducto = @id",
                    new[] { new SQLiteParameter("@id", id) });
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Registra un movimiento de inventario manualmente (útil al crear productos con stock inicial
        /// o cuando se detecta un cambio de stock al editar).
        /// </summary>
        public static void RegistrarMovimientoInventario(int idProducto, string tipo, int cantidad,
            int stockAnterior, int stockNuevo, string motivo)
        {
            try
            {
                string sql = @"INSERT INTO Movimientos_Inventario
                               (IdProducto, TipoMovimiento, Cantidad, StockAnterior, StockNuevo, Motivo, IdUsuario)
                               VALUES (@id, @tipo, @cant, @ant, @nue, @mot, @user)";
                DatabaseHelper.ExecuteNonQuery(sql, new[] {
                    new SQLiteParameter("@id", idProducto),
                    new SQLiteParameter("@tipo", tipo),
                    new SQLiteParameter("@cant", cantidad),
                    new SQLiteParameter("@ant", stockAnterior),
                    new SQLiteParameter("@nue", stockNuevo),
                    new SQLiteParameter("@mot", motivo),
                    new SQLiteParameter("@user", SesionActual.UsuarioActivo.IdUsuario)
                });
            }
            catch { /* no detener el flujo principal por un error de auditoría */ }
        }

        /// <summary>
        /// Obtiene el historial de movimientos de inventario de un producto, ordenado del más reciente al más antiguo.
        /// </summary>
        public static List<MovimientoInventario> ObtenerHistorialMovimientos(int idProducto)
        {
            string sql = @"SELECT m.*, u.NombreCompleto AS NombreUsuario, p.Nombre AS NombreProducto
                           FROM Movimientos_Inventario m
                           LEFT JOIN Usuarios u ON m.IdUsuario = u.IdUsuario
                           LEFT JOIN Productos p ON m.IdProducto = p.IdProducto
                           WHERE m.IdProducto = @id
                           ORDER BY m.FechaHora DESC";

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new[] { new SQLiteParameter("@id", idProducto) });
            var lista = new List<MovimientoInventario>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new MovimientoInventario
                {
                    IdMovimiento = Convert.ToInt32(row["IdMovimiento"]),
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    NombreProducto = row["NombreProducto"] != DBNull.Value ? row["NombreProducto"].ToString() : "",
                    TipoMovimiento = row["TipoMovimiento"].ToString(),
                    Cantidad = Convert.ToInt32(row["Cantidad"]),
                    StockAnterior = Convert.ToInt32(row["StockAnterior"]),
                    StockNuevo = Convert.ToInt32(row["StockNuevo"]),
                    Motivo = row["Motivo"] != DBNull.Value ? row["Motivo"].ToString() : "",
                    NombreUsuario = row["NombreUsuario"] != DBNull.Value ? row["NombreUsuario"].ToString() : "—",
                    FechaHora = Convert.ToDateTime(row["FechaHora"])
                });
            }
            return lista;
        }

        private static bool ExisteCodigoBarras(string codigo, int? idProductoExcluir = null)
        {
            string sql = "SELECT COUNT(*) FROM Productos WHERE CodigoBarras = @codigo";
            var parameters = new List<SQLiteParameter> { new SQLiteParameter("@codigo", codigo) };

            if (idProductoExcluir.HasValue)
            {
                sql += " AND IdProducto != @id";
                parameters.Add(new SQLiteParameter("@id", idProductoExcluir.Value));
            }

            var result = DatabaseHelper.ExecuteScalar(sql, parameters.ToArray());
            return Convert.ToInt32(result) > 0;
        }

        private static bool ExisteSKU(string sku, int? idProductoExcluir = null)
        {
            string sql = "SELECT COUNT(*) FROM Productos WHERE SKU = @sku AND SKU != ''";
            var parameters = new List<SQLiteParameter> { new SQLiteParameter("@sku", sku) };

            if (idProductoExcluir.HasValue)
            {
                sql += " AND IdProducto != @id";
                parameters.Add(new SQLiteParameter("@id", idProductoExcluir.Value));
            }

            var result = DatabaseHelper.ExecuteScalar(sql, parameters.ToArray());
            return Convert.ToInt32(result) > 0;
        }
    }
}
