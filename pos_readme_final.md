# 🏪 Sistema POS - Punto de Venta Completo para Windows

## 📌 Descripción

Sistema completo de Punto de Venta (POS) para Windows, desarrollado en C# con Windows Forms y SQLite. Incluye control de inventario, ventas, reportes y gestión de usuarios con roles.

## ✨ Características Principales

### 🎯 Para Cajeros
- Interfaz rápida de punto de venta
- Escaneo de códigos de barras
- Cálculo automático de totales e IVA
- Múltiples métodos de pago (Efectivo, Tarjeta, Transferencia)
- Impresión de tickets
- Actualización automática de inventario

### 👨‍💼 Para Supervisores
- Todo lo de cajeros +
- Gestión completa de productos
- Control de inventario en tiempo real
- Reportes de ventas y estadísticas
- Gestión de usuarios y permisos
- Backup de base de datos
- Corte de caja

## 🛠️ Tecnologías

- **Lenguaje:** C# (.NET Framework 4.7.2+)
- **UI:** Windows Forms
- **Base de Datos:** SQLite
- **IDE:** Visual Studio 2019/2022

## 📦 Contenido del Proyecto

```
📁 SistemaPOS/
├── 📂 Data/                    # Capa de acceso a datos
├── 📂 Models/                  # Modelos de negocio
├── 📂 Services/                # Lógica de negocio
├── 📂 Forms/                   # Interfaces de usuario
├── 📄 Program.cs               # Punto de entrada
└── 📄 SistemaPOS.csproj        # Archivo del proyecto
```

## 🚀 Instalación Rápida

### Requisitos Previos
- Windows 10 o superior
- Visual Studio 2019/2022
- .NET Framework 4.7.2 o superior

### Pasos de Instalación

1. **Descomprimir el ZIP**
   ```
   Extraer el archivo SistemaPOS.zip
   ```

2. **Abrir el Proyecto**
   ```
   Doble click en SistemaPOS.sln
   ```

3. **Restaurar Paquetes NuGet**
   ```
   Visual Studio lo hará automáticamente
   Si no: Click derecho en Solución → Restaurar paquetes NuGet
   ```

4. **Compilar**
   ```
   Presionar F6 o Compilar → Compilar Solución
   ```

5. **Ejecutar**
   ```
   Presionar F5 o click en el botón ▶️ Iniciar
   ```

## 🔐 Primer Inicio de Sesión

Al iniciar por primera vez, usa estas credenciales (base recién creada):

```
Usuario: admin
Contraseña: admin
```

Si la base de datos fue creada con otra versión del sistema, prueba también `admin123`.

**⚠️ IMPORTANTE:** Cambia esta contraseña después del primer acceso.

---

## 🧪 Para correr pruebas

1. **Compilar:** En Visual Studio → F6 (Compilar solución). En Cursor/VS Code, la tarea **"build"** usa `msbuild` (debe estar en el PATH; si no, compila desde Visual Studio).
2. **Ejecutar:** F5 o elegir **"Iniciar SistemaPOS (Debug)"** en el selector de depuración. La app usa la base `pos_database.db` en la carpeta del .exe (`bin\Debug` o `bin\Release`).
3. **Flujo sugerido para probar:**
   - Login con **admin** / **admin**.
   - **Terminal Venta:** crear ítems en el carrito → enviar a cola (crea un pedido PENDIENTE).
   - En la **cola de cobro**, doble clic en un pedido → **Cobrar** (abre CobrarForm con ese pedido). Al confirmar, la venta se registra y el pedido pasa a PAGADO.
   - Revisar **Reportes** y **Inventario** para ver ventas y movimientos.

## 📚 Guía de Uso

### Para Cajeros

1. **Iniciar Sesión**
   - Ingresar usuario y contraseña
   
2. **Realizar una Venta**
   - Escanear o buscar producto
   - Modificar cantidades si es necesario
   - Click en "COBRAR"
   - Seleccionar método de pago
   - Ingresar monto recibido (si es efectivo)
   - Confirmar venta
   - Se imprime el ticket automáticamente

3. **Cancelar Venta**
   - Click en "CANCELAR VENTA"
   - Confirmar la acción

### Para Supervisores

#### Gestión de Inventario
1. Click en "INVENTARIO"
2. Ver lista de productos
3. Opciones disponibles:
   - ➕ Nuevo Producto
   - ✏️ Editar producto existente
   - 📦 Ajustar stock
   - 🗑️ Eliminar producto

#### Reportes
1. Click en "REPORTES"
2. Seleccionar tipo de reporte:
   - Ventas por período
   - Productos más vendidos
   - Estado de inventario
   - Corte de caja
3. Seleccionar rango de fechas
4. Click en "Generar"

#### Gestión de Usuarios
1. Click en "USUARIOS"
2. Ver lista de usuarios
3. Opciones:
   - ➕ Crear usuario nuevo
   - ✏️ Editar información
   - 🔑 Cambiar contraseña
   - 🚫 Desactivar usuario

#### Backup
1. Click en "BACKUP"
2. Seleccionar ubicación
3. Guardar archivo .db

## 🔧 Configuración

### Cambiar Tasa de IVA

En `Forms/PuntoVentaForm.cs`, línea ~18:
```csharp
private decimal tasaIVA = 0.16m; // Cambiar aquí
```

Ejemplos:
- México: 0.16 (16%)
- Chile: 0.19 (19%)
- España: 0.21 (21%)

### Configurar Impresora Térmica

En `Forms/CobrarForm.cs`, método `ImprimirTicket`:
```csharp
// Cambiar el nombre por tu impresora
pd.PrinterSettings.PrinterName = "TM-T20";
```

### Cambiar Ubicación de la Base de Datos

En `Data/DatabaseHelper.cs`:
```csharp
private static string dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "SistemaPOS",
    "pos_database.db"
);
```

## 🖨️ Hardware Recomendado

### Escáner de Código de Barras
- Tipo: USB HID (modo teclado)
- Recomendado: Cualquier lector USB genérico
- Configuración: Enviar Enter después del código
- **No requiere configuración en la aplicación**

### Impresora Térmica
- Recomendado: Epson TM-T20, TM-T88
- Alternativas: Star, Custom, Bixolon
- Ancho de papel: 80mm
- Conexión: USB o Ethernet

## 📊 Base de Datos

### Ubicación
```
[Carpeta del proyecto]/bin/Debug/pos_database.db
```

### Backup Manual
1. Copiar el archivo `pos_database.db`
2. Guardar en ubicación segura
3. Para restaurar: reemplazar el archivo

### Backup desde la Aplicación
1. Login como Supervisor
2. Click en "BACKUP"
3. Seleccionar ubicación y guardar

## 🔒 Seguridad

- ✅ Contraseñas hasheadas con SHA256
- ✅ Control de acceso por roles
- ✅ Registro de actividad de usuarios
- ✅ Sesión única por aplicación
- ✅ Validación en todas las operaciones

## 🐛 Solución de Problemas

### La aplicación no inicia
```
Verificar:
1. .NET Framework instalado
2. Permisos de escritura en carpeta
3. Ejecutar como Administrador
```

### Errores de SQLite
```
Solución:
1. Herramientas → NuGet → Consola
2. Ejecutar: Install-Package System.Data.SQLite.Core -Reinstall
```

### El escáner no funciona
```
Verificar:
1. Escáner en modo USB HID
2. Probar en Bloc de notas
3. Configurar envío de Enter
```

### No imprime tickets
```
Verificar:
1. Impresora instalada en Windows
2. Nombre correcto en el código
3. Driver actualizado
```

## 📈 Escalabilidad

### Para Múltiples Cajas
Migrar a SQL Server:
1. Instalar SQL Server Express
2. Modificar `DatabaseHelper.cs`
3. Cambiar connection string
4. Conectar todas las cajas a la misma BD

### Agregar Módulos
El código está modular y preparado para:
- Sistema de descuentos
- Programa de puntos
- Gestión de proveedores
- Control de gastos
- Integración con facturación electrónica

## 📞 Soporte

### Estructura del Código
- `Data/` - Acceso a base de datos
- `Models/` - Clases de negocio
- `Services/` - Lógica de negocio
- `Forms/` - Interfaces de usuario

### Agregar Funcionalidades
El sistema es completamente modificable. Cada módulo es independiente y fácil de extender.

## 📝 Notas de la Versión

**Versión:** 1.0.0
**Fecha:** Diciembre 2024
**Estado:** Producción Ready

### Incluye
- ✅ 18 archivos de código fuente
- ✅ 9 interfaces de usuario
- ✅ 3 servicios de negocio
- ✅ Sistema completo de base de datos
- ✅ Documentación completa

### Probado en
- ✅ Windows 10
- ✅ Windows 11
- ✅ Visual Studio 2019
- ✅ Visual Studio 2022

## 🎓 Aprendizaje

Este proyecto es ideal para:
- Aprender Windows Forms
- Entender arquitectura en capas
- Practicar SQLite
- Desarrollar sistemas de negocio reales

## 📄 Licencia

Código abierto. Uso libre para modificación y distribución.

## 🙏 Créditos

Sistema desarrollado con las mejores prácticas de programación en C# y Windows Forms.

---

## 🎉 ¡Listo para Usar!

El sistema está **100% completo** y **listo para producción**.

**Instalación:** 5 minutos
**Configuración:** 10 minutos
**Capacitación:** 30 minutos

### Próximos Pasos

1. ✅ Descomprimir el proyecto
2. ✅ Abrir en Visual Studio
3. ✅ Compilar y ejecutar
4. ✅ Cambiar contraseña admin
5. ✅ Agregar productos
6. ✅ Crear usuarios cajeros
7. ✅ Configurar impresora
8. ✅ ¡Empezar a vender!

---

**¿Necesitas ayuda?** Revisa la documentación completa en `ESTRUCTURA_COMPLETA_DEL_PROYECTO.md`

**¿Problemas?** Consulta la sección de solución de problemas o revisa `Guía de Instalación.md`

## 🚀 ¡Éxito con tu negocio!

---

Desarrollado con ❤️ para pequeñas y medianas empresas