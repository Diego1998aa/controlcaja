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
    }
}