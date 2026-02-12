using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SistemaPOS.Data;
using SistemaPOS.Models;

namespace SistemaPOS.Services
{
    public static class PedidoService
    {
        /// <summary>
        /// Crea un nuevo pedido en estado PENDIENTE.
        /// </summary>
        public static int CrearPedido(int idVendedor, List<ItemVenta> items, int? idCliente = null)
        {
            int idPedido = 0;
            decimal total = 0;
            foreach (var item in items) total += item.Subtotal;

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertar Cabecera del Pedido
                        string sqlPedido = @"INSERT INTO Pedidos (IdUsuarioVendedor, IdCliente, Total, Estado, Fecha) 
                                           VALUES (@vendedor, @cliente, @total, 'PENDIENTE', CURRENT_TIMESTAMP);
                                           SELECT last_insert_rowid();";
                        
                        using (var cmd = new SQLiteCommand(sqlPedido, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@vendedor", idVendedor);
                            cmd.Parameters.AddWithValue("@cliente", idCliente ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@total", total);
                            idPedido = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 2. Insertar Detalles
                        string sqlDetalle = @"INSERT INTO Detalle_Pedido (IdPedido, IdProducto, Cantidad, PrecioUnitario, Subtotal) 
                                            VALUES (@idPedido, @idProd, @cant, @precio, @subtotal)";

                        foreach (var item in items)
                        {
                            using (var cmd = new SQLiteCommand(sqlDetalle, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@idPedido", idPedido);
                                cmd.Parameters.AddWithValue("@idProd", item.Producto.IdProducto);
                                cmd.Parameters.AddWithValue("@cant", item.Cantidad);
                                cmd.Parameters.AddWithValue("@precio", item.Producto.PrecioVenta);
                                cmd.Parameters.AddWithValue("@subtotal", item.Subtotal);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            return idPedido;
        }

        /// <summary>
        /// Obtiene todos los pedidos con estado PENDIENTE.
        /// </summary>
        public static List<Pedido> ObtenerPendientes()
        {
            var lista = new List<Pedido>();
            string sql = @"SELECT p.*, u.NombreUsuario 
                           FROM Pedidos p 
                           LEFT JOIN Usuarios u ON p.IdUsuarioVendedor = u.IdUsuario 
                           WHERE p.Estado = 'PENDIENTE' 
                           ORDER BY p.Fecha DESC";

            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Pedido
                {
                    IdPedido = Convert.ToInt32(row["IdPedido"]),
                    IdUsuarioVendedor = Convert.ToInt32(row["IdUsuarioVendedor"]),
                    IdCliente = row["IdCliente"] != DBNull.Value ? Convert.ToInt32(row["IdCliente"]) : (int?)null,
                    Fecha = Convert.ToDateTime(row["Fecha"]),
                    Total = Convert.ToDecimal(row["Total"]),
                    Estado = row["Estado"].ToString(),
                    NombreVendedor = row["NombreUsuario"].ToString()
                });
            }
            return lista;
        }

        /// <summary>
        /// Obtiene un pedido específico con sus detalles.
        /// </summary>
        public static Pedido ObtenerPedidoPorId(int idPedido)
        {
            Pedido pedido = null;
            string sql = "SELECT * FROM Pedidos WHERE IdPedido = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new[] { new SQLiteParameter("@id", idPedido) });

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                pedido = new Pedido
                {
                    IdPedido = Convert.ToInt32(row["IdPedido"]),
                    IdUsuarioVendedor = Convert.ToInt32(row["IdUsuarioVendedor"]),
                    IdCliente = row["IdCliente"] != DBNull.Value ? Convert.ToInt32(row["IdCliente"]) : (int?)null,
                    Fecha = Convert.ToDateTime(row["Fecha"]),
                    Total = Convert.ToDecimal(row["Total"]),
                    Estado = row["Estado"].ToString()
                };

                // Cargar detalles
                string sqlDet = @"SELECT dp.*, p.Nombre, p.CodigoBarras, p.SKU 
                                  FROM Detalle_Pedido dp 
                                  JOIN Productos p ON dp.IdProducto = p.IdProducto 
                                  WHERE dp.IdPedido = @id";
                
                DataTable dtDet = DatabaseHelper.ExecuteQuery(sqlDet, new[] { new SQLiteParameter("@id", idPedido) });
                foreach (DataRow r in dtDet.Rows)
                {
                    pedido.Detalles.Add(new DetallePedido
                    {
                        IdDetallePedido = Convert.ToInt32(r["IdDetallePedido"]),
                        IdPedido = Convert.ToInt32(r["IdPedido"]),
                        IdProducto = Convert.ToInt32(r["IdProducto"]),
                        Cantidad = Convert.ToInt32(r["Cantidad"]),
                        PrecioUnitario = Convert.ToDecimal(r["PrecioUnitario"]),
                        Subtotal = Convert.ToDecimal(r["Subtotal"]),
                        Producto = new Producto 
                        { 
                            IdProducto = Convert.ToInt32(r["IdProducto"]),
                            Nombre = r["Nombre"].ToString(),
                            CodigoBarras = r["CodigoBarras"].ToString(),
                            SKU = r["SKU"].ToString(),
                            PrecioVenta = Convert.ToDecimal(r["PrecioUnitario"]) // Precio al momento del pedido
                        }
                    });
                }
            }
            return pedido;
        }

        /// <summary>
        /// Cambia el estado de un pedido a CANCELADO.
        /// </summary>
        public static bool CancelarPedido(int idPedido)
        {
            string sql = "UPDATE Pedidos SET Estado = 'CANCELADO' WHERE IdPedido = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new[] { new SQLiteParameter("@id", idPedido) }) > 0;
        }

        /// <summary>
        /// Actualiza el estado de un pedido (ej. a PAGADO).
        /// </summary>
        public static bool ActualizarEstado(int idPedido, string nuevoEstado)
        {
            string sql = "UPDATE Pedidos SET Estado = @estado WHERE IdPedido = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new[] { 
                new SQLiteParameter("@estado", nuevoEstado),
                new SQLiteParameter("@id", idPedido) 
            }) > 0;
        }
    }
}