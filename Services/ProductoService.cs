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
            string query = "SELECT * FROM Productos WHERE CodigoBarras = @codigo OR SKU = @codigo";
            DataTable dt = DatabaseHelper.ExecuteQuery(query, new[] { new SQLiteParameter("@codigo", codigo) });
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Producto
                {
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    CodigoBarras = row["CodigoBarras"].ToString(),
                    Nombre = row["Nombre"].ToString(),
                    PrecioVenta = Convert.ToDecimal(row["PrecioVenta"]),
                    StockActual = Convert.ToInt32(row["StockActual"])
                };
            }
            return null;
        }

        public static List<Producto> ObtenerTodos(bool soloActivos = true)
        {
            List<Producto> lista = new List<Producto>();
            string sql = "SELECT p.*, c.NombreCategoria FROM Productos p LEFT JOIN Categorias c ON p.IdCategoria = c.IdCategoria";
            if (soloActivos) sql += " WHERE p.StockActual >= 0"; 
            
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Producto
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
                    NombreCategoria = row["NombreCategoria"].ToString()
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
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Categorias");
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
            try
            {
                string sql = @"INSERT INTO Productos (CodigoBarras, SKU, Nombre, Descripcion, PrecioCompra, PrecioVenta, StockActual, StockMinimo, IdCategoria) 
                               VALUES (@cod, @sku, @nom, @desc, @pcompra, @pventa, @stock, @min, @cat)";
                DatabaseHelper.ExecuteQuery(sql, new[] {
                    new SQLiteParameter("@cod", p.CodigoBarras),
                    new SQLiteParameter("@sku", p.SKU),
                    new SQLiteParameter("@nom", p.Nombre),
                    new SQLiteParameter("@desc", p.Descripcion),
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
            try
            {
                string sql = @"UPDATE Productos SET CodigoBarras=@cod, SKU=@sku, Nombre=@nom, Descripcion=@desc, 
                               PrecioCompra=@pcompra, PrecioVenta=@pventa, StockActual=@stock, StockMinimo=@min, IdCategoria=@cat 
                               WHERE IdProducto=@id";
                DatabaseHelper.ExecuteQuery(sql, new[] {
                    new SQLiteParameter("@cod", p.CodigoBarras),
                    new SQLiteParameter("@sku", p.SKU),
                    new SQLiteParameter("@nom", p.Nombre),
                    new SQLiteParameter("@desc", p.Descripcion),
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
            try { DatabaseHelper.ExecuteQuery("DELETE FROM Productos WHERE IdProducto = @id", new[]{new SQLiteParameter("@id", id)}); return true; } catch { return false; }
        }
    }
}