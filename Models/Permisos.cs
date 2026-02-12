using System;
using System.Collections.Generic;

namespace SistemaPOS.Models
{
    /// <summary>
    /// Enum que define los permisos granulares del sistema
    /// </summary>
    public enum Permiso
    {
        // Permisos de Ventas
        CrearPedidos,              // Vendedor puede crear pedidos
        ProcesarCobros,            // Cajera puede procesar cobros

        // Permisos de Productos
        VerInventario,             // Ver listado de productos
        EditarProductos,           // Modificar productos y precios
        AjustarStock,              // Ajustar inventario

        // Permisos de Usuarios
        VerUsuarios,               // Ver lista de usuarios
        CrearUsuarios,             // Crear nuevos usuarios
        EditarUsuarios,            // Modificar usuarios existentes
        CambiarContraseñas,        // Cambiar contraseña de otros usuarios
        CambiarPropiaContraseña,   // Solo cambiar propia contraseña

        // Permisos de Reportes
        VerReportes,               // Acceso a reportes

        // Permisos de Configuración
        ConfigurarSistema          // Acceso a configuración general
    }

    /// <summary>
    /// Clase estática que gestiona los roles y sus permisos
    /// </summary>
    public static class RolesPermisos
    {
        // Constantes para los roles del sistema
        public const string Vendedor = "Vendedor";
        public const string Cajera = "Cajera";
        public const string Supervisor = "Supervisor";

        /// <summary>
        /// Diccionario que mapea cada rol a sus permisos permitidos
        /// </summary>
        private static readonly Dictionary<string, List<Permiso>> permisosPorRol = new Dictionary<string, List<Permiso>>
        {
            {
                Vendedor, new List<Permiso>
                {
                    Permiso.CrearPedidos,
                    Permiso.VerInventario,
                    Permiso.CambiarPropiaContraseña
                }
            },
            {
                Cajera, new List<Permiso>
                {
                    Permiso.ProcesarCobros,
                    Permiso.VerInventario,
                    Permiso.CambiarPropiaContraseña
                }
            },
            {
                Supervisor, new List<Permiso>
                {
                    Permiso.CrearPedidos,
                    Permiso.ProcesarCobros,
                    Permiso.VerInventario,
                    Permiso.EditarProductos,
                    Permiso.AjustarStock,
                    Permiso.VerUsuarios,
                    Permiso.CrearUsuarios,
                    Permiso.EditarUsuarios,
                    Permiso.CambiarContraseñas,
                    Permiso.CambiarPropiaContraseña,
                    Permiso.VerReportes,
                    Permiso.ConfigurarSistema
                }
            }
        };

        /// <summary>
        /// Verifica si un rol tiene un permiso específico
        /// </summary>
        /// <param name="rol">El rol a verificar</param>
        /// <param name="permiso">El permiso a verificar</param>
        /// <returns>True si el rol tiene el permiso, False en caso contrario</returns>
        public static bool TienePermiso(string rol, Permiso permiso)
        {
            if (string.IsNullOrEmpty(rol) || !permisosPorRol.ContainsKey(rol))
                return false;

            return permisosPorRol[rol].Contains(permiso);
        }

        /// <summary>
        /// Obtiene la lista de roles disponibles en el sistema
        /// </summary>
        /// <returns>Lista de nombres de roles</returns>
        public static List<string> ObtenerRolesDisponibles()
        {
            return new List<string> { Vendedor, Cajera, Supervisor };
        }

        /// <summary>
        /// Obtiene todos los permisos de un rol
        /// </summary>
        /// <param name="rol">El rol del que se quieren obtener los permisos</param>
        /// <returns>Lista de permisos o lista vacía si el rol no existe</returns>
        public static List<Permiso> ObtenerPermisos(string rol)
        {
            if (string.IsNullOrEmpty(rol) || !permisosPorRol.ContainsKey(rol))
                return new List<Permiso>();

            return new List<Permiso>(permisosPorRol[rol]);
        }
    }
}
