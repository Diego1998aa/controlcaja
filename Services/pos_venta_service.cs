using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using SistemaPOS.Data;
using SistemaPOS.Models;

namespace SistemaPOS.Services
{
    public class VentaService
    {
        // Registrar venta completa
        public static int RegistrarVenta(List<ItemVenta> items, string metodoPago, decimal montoRecibido, 
                                         int idUsuarioCajero, decimal descuento = 0, decimal tasaIVA = 0)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Calcular totales
                        decimal subtotal = 0;
                        foreach (var item in items)
                        {
                            subtotal += item.Subtotal;
                        }

                        decimal iva = subtotal * tasaIVA;
                        decimal total = subtotal + iva - descuento;
                        decimal cambio = montoRecibido - total;

                        // Insertar venta
                        string sqlVenta = @"
                            INSERT INTO Ventas 
                            (id_usuario_cajero, subtotal, iva, descuento, total, metodo_pago, monto_recibido, monto_cambio)
                            VALUES (@cajero, @subtotal, @iva, @descuento, @total, @metodo, @recibido, @cambio);
                            SELECT last_insert_rowid();
                        ";

                        int idVenta;
                        using (var cmd = new SQLiteCommand(sqlVenta, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@cajero", idUsuarioCajero);
                            cmd.Parameters.AddWithValue("@subtotal", subtotal);
                            cmd.Parameters.AddWithValue("@iva", iva);
                            cmd.Parameters.AddWithValue("@descuento", descuento);
                            cmd.Parameters.AddWithValue("@total", total);
                            cmd.Parameters.AddWithValue("@metodo", metodoPago);
                            cmd.Parameters.AddWithValue("@recibido", montoRecibido);
                            cmd.Parameters.AddWithValue("@cambio", cambio);

                            idVenta = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Insertar detalles y actualizar stock
                        string sqlDetalle = @"
                            INSERT INTO Detalle_Venta 
                            (id_venta, id_producto, cantidad, precio_unitario, subtotal)
                            VALUES (@venta, @producto, @cantidad, @precio, @subtotal)
                        ";

                        string sqlUpdateStock = @"
                            UPDATE Productos 
                            SET stock_actual = stock_actual - @cantidad 
                            WHERE id_producto = @id
                        ";

                        string sqlMovimiento = @"
                            INSERT INTO Movimientos_Inventario 
                            (id_producto, tipo_movimiento, cantidad, stock_anterior, stock_nuevo, motivo, id_usuario)
                            VALUES (@producto, 'Venta', @cantidad, @stockAnt, @stockNuevo, @motivo, @usuario)
                        ";

                        foreach (var item in items)
                        {
                            // Insertar detalle
                            using (var cmd = new SQLiteCommand(sqlDetalle, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@venta", idVenta);
                                cmd.Parameters.AddWithValue("@producto", item.Producto.IdProducto);
                                cmd.Parameters.AddWithValue("@cantidad", item.Cantidad);
                                cmd.Parameters.AddWithValue("@precio", item.Producto.PrecioVenta);
                                cmd.Parameters.AddWithValue("@subtotal", item.Subtotal);
                                cmd.ExecuteNonQuery();
                            }

                            // Actualizar stock
                            int stockAnterior = item.Producto.StockActual;
                            using (var cmd = new SQLiteCommand(sqlUpdateStock, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@cantidad", item.Cantidad);
                                cmd.Parameters.AddWithValue("@id", item.Producto.IdProducto);
                                cmd.ExecuteNonQuery();
                            }

                            // Registrar movimiento
                            using (var cmd = new SQLiteCommand(sqlMovimiento, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@producto", item.Producto.IdProducto);
                                cmd.Parameters.AddWithValue("@cantidad", item.Cantidad);
                                cmd.Parameters.AddWithValue("@stockAnt", stockAnterior);
                                cmd.Parameters.AddWithValue("@stockNuevo", stockAnterior - item.Cantidad);
                                cmd.Parameters.AddWithValue("@motivo", $"Venta #{idVenta}");
                                cmd.Parameters.AddWithValue("@usuario", idUsuarioCajero);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return idVenta;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Obtener venta por ID
        public static Venta ObtenerVentaPorId(int idVenta)
        {
            string sql = @"
                SELECT v.*, u.nombre_completo as nombre_cajero
                FROM Ventas v
                INNER JOIN Usuarios u ON v.id_usuario_cajero = u.id_usuario
                WHERE v.id_venta = @id
            ";

            var parameters = new SQLiteParameter[] { new SQLiteParameter("@id", idVenta) };
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);

            if (dt.Rows.Count > 0)
            {
                return MapearVenta(dt.Rows[0]);
            }

            return null;
        }

        // Obtener detalle de venta
        public static List<DetalleVenta> ObtenerDetalleVenta(int idVenta)
        {
            List<DetalleVenta> detalles = new List<DetalleVenta>();
            string sql = @"
                SELECT dv.*, p.nombre as nombre_producto
                FROM Detalle_Venta dv
                INNER JOIN Productos p ON dv.id_producto = p.id_producto
                WHERE dv.id_venta = @id
            ";

            var parameters = new SQLiteParameter[] { new SQLiteParameter("@id", idVenta) };
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);

            foreach (DataRow row in dt.Rows)
            {
                detalles.Add(new DetalleVenta
                {
                    IdDetalle = Convert.ToInt32(row["id_detalle"]),
                    IdVenta = Convert.ToInt32(row["id_venta"]),
                    IdProducto = Convert.ToInt32(row["id_producto"]),
                    NombreProducto = row["nombre_producto"].ToString(),
                    Cantidad = Convert.ToInt32(row["cantidad"]),
                    PrecioUnitario = Convert.ToDecimal(row["precio_unitario"]),
                    Subtotal = Convert.ToDecimal(row["subtotal"])
                });
            }

            return detalles;
        }

        // Obtener ventas por fecha
        public static List<Venta> ObtenerVentasPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            List<Venta> ventas = new List<Venta>();
            string sql = @"
                SELECT v.*, u.nombre_completo as nombre_cajero
                FROM Ventas v
                INNER JOIN Usuarios u ON v.id_usuario_cajero = u.id_usuario
                WHERE DATE(v.fecha_hora) BETWEEN DATE(@inicio) AND DATE(@fin)
                ORDER BY v.fecha_hora DESC
            ";

            var parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@inicio", fechaInicio.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@fin", fechaFin.ToString("yyyy-MM-dd"))
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);

            foreach (DataRow row in dt.Rows)
            {
                ventas.Add(MapearVenta(row));
            }

            return ventas;
        }

        // Obtener ventas del día
        public static List<Venta> ObtenerVentasDelDia(int? idUsuario = null)
        {
            List<Venta> ventas = new List<Venta>();
            string sql = @"
                SELECT v.*, u.nombre_completo as nombre_cajero
                FROM Ventas v
                INNER JOIN Usuarios u ON v.id_usuario_cajero = u.id_usuario
                WHERE DATE(v.fecha_hora) = DATE('now')
            ";

            if (idUsuario.HasValue)
                sql += " AND v.id_usuario_cajero = @usuario";

            sql += " ORDER BY v.fecha_hora DESC";

            var parameters = idUsuario.HasValue
                ? new SQLiteParameter[] { new SQLiteParameter("@usuario", idUsuario.Value) }
                : null;

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);

            foreach (DataRow row in dt.Rows)
            {
                ventas.Add(MapearVenta(row));
            }

            return ventas;
        }

        // Obtener resumen de ventas
        public static DataTable ObtenerResumenVentas(DateTime fechaInicio, DateTime fechaFin)
        {
            string sql = @"
                SELECT 
                    DATE(fecha_hora) as Fecha,
                    COUNT(*) as NumeroVentas,
                    SUM(total) as TotalVentas,
                    SUM(CASE WHEN metodo_pago = 'Efectivo' THEN total ELSE 0 END) as TotalEfectivo,
                    SUM(CASE WHEN metodo_pago = 'Tarjeta' THEN total ELSE 0 END) as TotalTarjeta,
                    SUM(CASE WHEN metodo_pago = 'Transferencia' THEN total ELSE 0 END) as TotalTransferencia
                FROM Ventas
                WHERE DATE(fecha_hora) BETWEEN DATE(@inicio) AND DATE(@fin)
                AND estado = 'Completada'
                GROUP BY DATE(fecha_hora)
                ORDER BY Fecha DESC
            ";

            var parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@inicio", fechaInicio.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@fin", fechaFin.ToString("yyyy-MM-dd"))
            };

            return DatabaseHelper.ExecuteQuery(sql, parameters);
        }

        // Obtener productos más vendidos
        public static DataTable ObtenerProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin, int top = 10)
        {
            string sql = @"
                SELECT 
                    p.nombre as Producto,
                    SUM(dv.cantidad) as CantidadVendida,
                    SUM(dv.subtotal) as TotalVentas
                FROM Detalle_Venta dv
                INNER JOIN Ventas v ON dv.id_venta = v.id_venta
                INNER JOIN Productos p ON dv.id_producto = p.id_producto
                WHERE DATE(v.fecha_hora) BETWEEN DATE(@inicio) AND DATE(@fin)
                AND v.estado = 'Completada'
                GROUP BY dv.id_producto, p.nombre
                ORDER BY CantidadVendida DESC
                LIMIT @top
            ";

            var parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@inicio", fechaInicio.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@fin", fechaFin.ToString("yyyy-MM-dd")),
                new SQLiteParameter("@top", top)
            };

            return DatabaseHelper.ExecuteQuery(sql, parameters);
        }

        // Cancelar venta
        public static bool CancelarVenta(int idVenta, int idUsuario)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Obtener detalles de la venta
                        var detalles = ObtenerDetalleVenta(idVenta);

                        // Devolver stock
                        foreach (var detalle in detalles)
                        {
                            string sqlStock = @"
                                UPDATE Productos 
                                SET stock_actual = stock_actual + @cantidad 
                                WHERE id_producto = @id
                            ";
                            using (var cmd = new SQLiteCommand(sqlStock, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                                cmd.Parameters.AddWithValue("@id", detalle.IdProducto);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Marcar venta como cancelada
                        string sqlVenta = "UPDATE Ventas SET estado = 'Cancelada' WHERE id_venta = @id";
                        using (var cmd = new SQLiteCommand(sqlVenta, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", idVenta);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        // Mapear DataRow a Venta
        private static Venta MapearVenta(DataRow row)
        {
            return new Venta
            {
                IdVenta = Convert.ToInt32(row["id_venta"]),
                IdUsuarioCajero = Convert.ToInt32(row["id_usuario_cajero"]),
                NombreCajero = row["nombre_cajero"].ToString(),
                FechaHora = Convert.ToDateTime(row["fecha_hora"]),
                Subtotal = Convert.ToDecimal(row["subtotal"]),
                IVA = Convert.ToDecimal(row["iva"]),
                Descuento = Convert.ToDecimal(row["descuento"]),
                Total = Convert.ToDecimal(row["total"]),
                MetodoPago = row["metodo_pago"].ToString(),
                MontoRecibido = Convert.ToDecimal(row["monto_recibido"]),
                MontoCambio = Convert.ToDecimal(row["monto_cambio"]),
                Estado = row["estado"].ToString()
            };
        }
    }
}