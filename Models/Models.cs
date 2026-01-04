using System;
using System.Collections.Generic;

namespace SistemaPOS.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string Rol { get; set; }
        public bool Activo { get; set; }
    }

    public class Producto
    {
        public int IdProducto { get; set; }
        public string CodigoBarras { get; set; }
        public string SKU { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int IdCategoria { get; set; }
    }

    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; }
    }

    public class ItemVenta
    {
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; private set; }

        public ItemVenta(Producto producto, int cantidad)
        {
            Producto = producto;
            Cantidad = cantidad;
            CalcularSubtotal();
        }

        public void IncrementarCantidad() { Cantidad++; CalcularSubtotal(); }
        public void CalcularSubtotal() { Subtotal = Producto.PrecioVenta * Cantidad; }
    }

    public static class SesionActual
    {
        public static Usuario UsuarioActivo { get; set; }
        public static bool EsSupervisor() => UsuarioActivo != null && UsuarioActivo.Rol == "Supervisor";
        public static void CerrarSesion() => UsuarioActivo = null;
    }
}