using System;
using System.Collections.Generic;

namespace SistemaPOS.Models
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int IdUsuarioVendedor { get; set; }
        public int? IdCliente { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } // 'PENDIENTE', 'PAGADO', 'CANCELADO'

        // Propiedades extendidas para la UI
        public string NombreVendedor { get; set; }
        public List<DetallePedido> Detalles { get; set; }

        public Pedido()
        {
            Detalles = new List<DetallePedido>();
        }
    }
}