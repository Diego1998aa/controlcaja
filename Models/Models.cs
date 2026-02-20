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
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
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
        public string NombreCategoria { get; set; }
        public string Estado { get; set; }

        public Producto()
        {
            Estado = "Activo";
        }

        // Métodos auxiliares
        public bool TieneBajoStock() { return StockActual > 0 && StockActual <= StockMinimo; }
        public bool SinStock() { return StockActual <= 0; }
    }

    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; }
    }

    public class MovimientoInventario
    {
        public int IdMovimiento { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string TipoMovimiento { get; set; }
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public string Motivo { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaHora { get; set; }
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

        /// <summary>
        /// Verifica si el usuario actual es Supervisor
        /// </summary>
        public static bool EsSupervisor()
        {
            return UsuarioActivo != null && UsuarioActivo.Rol == RolesPermisos.Supervisor;
        }

        /// <summary>
        /// Verifica si el usuario actual es Vendedor
        /// </summary>
        public static bool EsVendedor()
        {
            return UsuarioActivo != null && UsuarioActivo.Rol == RolesPermisos.Vendedor;
        }

        /// <summary>
        /// Verifica si el usuario actual es Cajera
        /// </summary>
        public static bool EsCajera()
        {
            return UsuarioActivo != null && UsuarioActivo.Rol == RolesPermisos.Cajera;
        }

        /// <summary>
        /// Verifica si el usuario actual tiene un permiso específico
        /// </summary>
        /// <param name="permiso">El permiso a verificar</param>
        /// <returns>True si tiene el permiso, False en caso contrario</returns>
        public static bool TienePermiso(Permiso permiso)
        {
            if (UsuarioActivo == null)
                return false;

            return RolesPermisos.TienePermiso(UsuarioActivo.Rol, permiso);
        }

        /// <summary>
        /// Cierra la sesión del usuario actual
        /// </summary>
        public static void CerrarSesion()
        {
            UsuarioActivo = null;
        }
    }
}
