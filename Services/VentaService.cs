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
                                         int idUsuarioCajero, decimal descuento = 0, decimal tasaIVA = 0, string numeroVoucher = null, int? idPedido = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Calcular totales (Precios son Brutos / IVA incluido)
                        decimal totalBruto = 0;
                        foreach (var item in items)
                        {
                            totalBruto += item.Subtotal;
                        }

                        decimal total = totalBruto - descuento;
                        decimal subtotalNeto = total / (1 + tasaIVA);
                        decimal iva = total - subtotalNeto;
                        decimal cambio = montoRecibido - total;

                        // Insertar venta
                        string sqlVenta = @"
                            INSERT INTO Ventas 
                            (IdUsuarioCajero, Subtotal, IVA, Descuento, Total, MetodoPago, MontoRecibido, MontoCambio, NumeroVoucher, IdPedido)
                            VALUES (@cajero, @subtotal, @iva, @descuento, @total, @metodo, @recibido, @cambio, @voucher, @idPedido);
                            SELECT last_insert_rowid();
                        ";

                        int idVenta;
                        using (var cmd = new SQLiteCommand(sqlVenta, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@cajero", idUsuarioCajero);
                            cmd.Parameters.AddWithValue("@subtotal", subtotalNeto); // Guardamos el Neto
                            cmd.Parameters.AddWithValue("@iva", iva);
                            cmd.Parameters.AddWithValue("@descuento", descuento);
                            cmd.Parameters.AddWithValue("@total", total);
                            cmd.Parameters.AddWithValue("@metodo", metodoPago);
                            cmd.Parameters.AddWithValue("@recibido", montoRecibido);
                            cmd.Parameters.AddWithValue("@cambio", cambio);
                            cmd.Parameters.AddWithValue("@voucher", (object)numeroVoucher ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@idPedido", (object)idPedido ?? DBNull.Value);

                            idVenta = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Insertar detalles y actualizar stock
                        string sqlDetalle = @"
                            INSERT INTO Detalle_Venta 
                            (IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                            VALUES (@venta, @producto, @cantidad, @precio, @subtotal)
                        ";

                        string sqlUpdateStock = @"
                            UPDATE Productos 
                            SET StockActual = StockActual - @cantidad 
                            WHERE IdProducto = @id
                        ";

                        string sqlMovimiento = @"
                            INSERT INTO Movimientos_Inventario 
                            (IdProducto, TipoMovimiento, Cantidad, StockAnterior, StockNuevo, Motivo, IdUsuario)
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
                                cmd.Parameters.AddWithValue("@motivo",  idUsuarioCajero);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Si viene de un Pedido, actualizar estado a PAGADO
                        if (idPedido.HasValue)
                        {
                            string sqlUpdatePedido = "UPDATE Pedidos SET Estado = 'PAGADO' WHERE IdPedido = @idPedido";
                            using (var cmd = new SQLiteCommand(sqlUpdatePedido, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@idPedido", idPedido.Value);
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
            // Asegurar rango completo de fechas
            string inicioStr = fechaInicio.Date.ToString("yyyy-MM-dd 00:00:00");
            string finStr = fechaFin.Date.ToString("yyyy-MM-dd 23:59:59");

            string sql = @"
                SELECT 
                    STRFTIME('%Y-%m-%d', FechaHora) as Fecha,
                    COUNT(*) as NumeroVentas,
                    SUM(Total) as TotalVentas,
                    SUM(CASE WHEN MetodoPago = 'Efectivo' THEN Total ELSE 0 END) as TotalEfectivo,
                    SUM(CASE WHEN MetodoPago = 'Tarjeta' THEN Total ELSE 0 END) as TotalTarjeta,
                    SUM(CASE WHEN MetodoPago = 'Transferencia' THEN Total ELSE 0 END) as TotalTransferencia
                FROM Ventas
                WHERE FechaHora BETWEEN @inicio AND @fin
                GROUP BY STRFTIME('%Y-%m-%d', FechaHora)
                ORDER BY Fecha DESC
            ";

            var parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@inicio", inicioStr),
                new SQLiteParameter("@fin", finStr)
            };

            return DatabaseHelper.ExecuteQuery(sql, parameters);
        }

        // Obtener detalle de todas las ventas
        public static DataTable ObtenerVentasDetalladas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            string sql = @"
                SELECT 
                    v.IdVenta as 'Folio',
                    STRFTIME('%Y-%m-%d %H:%M', v.FechaHora) as 'Fecha',
                    u.NombreUsuario as 'Cajero',
                    v.MetodoPago as 'Método Pago',
                    v.Total,
                    v.NumeroVoucher as 'Voucher'
                FROM Ventas v
                LEFT JOIN Usuarios u ON v.IdUsuarioCajero = u.IdUsuario
            ";

            List<SQLiteParameter> parameters = new List<SQLiteParameter>();

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                sql += " WHERE v.FechaHora BETWEEN @inicio AND @fin";
                parameters.Add(new SQLiteParameter("@inicio", fechaInicio.Value.Date.ToString("yyyy-MM-dd 00:00:00")));
                parameters.Add(new SQLiteParameter("@fin", fechaFin.Value.Date.ToString("yyyy-MM-dd 23:59:59")));
            }

            sql += " ORDER BY v.FechaHora DESC";

            return DatabaseHelper.ExecuteQuery(sql, parameters.ToArray());
        }
    }
}
