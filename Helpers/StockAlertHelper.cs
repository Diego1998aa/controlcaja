using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SistemaPOS.Models;
using SistemaPOS.Services;

namespace SistemaPOS.Helpers
{
    /// <summary>
    /// Helper para gestionar alertas de stock bajo
    /// </summary>
    public static class StockAlertHelper
    {
        /// <summary>
        /// Muestra alertas de productos con stock bajo al supervisor
        /// </summary>
        public static void MostrarAlertasStock()
        {
            if (!SesionActual.TienePermiso(Permiso.EditarProductos))
                return; // Solo supervisores ven alertas

            var productosBajoStock = ProductoService.ObtenerConBajoStock();

            if (productosBajoStock.Count > 0)
            {
                string mensaje = string.Format(
                    "ALERTA: Hay {0} producto(s) con stock bajo o agotado:\n\n",
                    productosBajoStock.Count
                );

                int count = 0;
                foreach (var p in productosBajoStock.Take(10)) // Máximo 10 en la notificación
                {
                    string estado = p.SinStock() ? "SIN STOCK" : "STOCK BAJO";
                    mensaje += string.Format("• {0} ({1}): {2} unidades\n",
                        p.Nombre, estado, p.StockActual);
                    count++;
                }

                if (productosBajoStock.Count > 10)
                {
                    mensaje += string.Format("\n... y {0} producto(s) más",
                        productosBajoStock.Count - 10);
                }

                mensaje += "\n\n¿Desea ir al inventario para revisar?";

                DialogResult result = MessageBox.Show(mensaje,
                    "Alerta de Stock",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    var inventarioForm = new Forms.InventarioForm();
                    inventarioForm.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Obtiene un resumen del estado de stock
        /// </summary>
        public static string ObtenerResumenStock()
        {
            var todos = ProductoService.ObtenerTodos();
            var sinStock = todos.Where(p => p.SinStock()).Count();
            var bajoStock = todos.Where(p => p.TieneBajoStock()).Count();

            return string.Format("Stock: {0} agotados, {1} bajo mínimo", sinStock, bajoStock);
        }
    }
}
