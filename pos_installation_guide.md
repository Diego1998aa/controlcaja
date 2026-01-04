# Guía de Instalación y Configuración - Sistema POS

## Requisitos del Sistema

### Hardware
- **Computadora**: Windows 10 o superior
- **RAM**: Mínimo 4GB
- **Disco Duro**: 500MB de espacio disponible
- **Escáner de Código de Barras**: USB (tipo pistola)
- **Impresora Térmica**: USB/Ethernet (recomendado: Epson TM-T20, TM-T88 o similar)

### Software
- Windows 10/11
- .NET Framework 4.7.2 o superior (viene preinstalado en Windows 10/11)
- Visual Studio 2019/2022 (para desarrollo)

## Paso 1: Instalar Visual Studio

1. Descarga **Visual Studio Community** (gratis) desde: https://visualstudio.microsoft.com/
2. Durante la instalación, selecciona:
   - ✅ Desarrollo de escritorio de .NET
   - ✅ Herramientas de datos y almacenamiento

## Paso 2: Instalar SQLite

### Opción A: A través de NuGet (Recomendado)
En Visual Studio, una vez creado el proyecto:
1. Click derecho en el proyecto → "Administrar paquetes NuGet"
2. Buscar e instalar:
   - `System.Data.SQLite.Core` (versión más reciente)
   - `System.Data.SQLite.EF6` (opcional, para Entity Framework)

### Opción B: Instalación Manual
1. Descargar desde: https://system.data.sqlite.org/downloads/
2. Instalar el bundle completo para .NET Framework

## Paso 3: Crear el Proyecto

### 3.1 En Visual Studio
1. Archivo → Nuevo → Proyecto
2. Seleccionar "Aplicación de Windows Forms (.NET Framework)"
3. Nombre del proyecto: `SistemaPOS`
4. Framework: .NET Framework 4.7.2 o superior
5. Click en "Crear"

### 3.2 Estructura de Carpetas
Crear las siguientes carpetas en el proyecto:
```
SistemaPOS/
├── Data/
│   └── DatabaseHelper.cs
├── Models/
│   └── Models.cs
├── Services/
│   ├── UsuarioService.cs
│   ├── ProductoService.cs
│   └── VentaService.cs
└── Forms/
    ├── LoginForm.cs
    ├── MainForm.cs
    ├── PuntoVentaForm.cs
    ├── CobrarForm.cs
    ├── InventarioForm.cs
    ├── ProductoEditForm.cs
    ├── AjustarStockForm.cs
    ├── ReportesForm.cs
    └── UsuariosForm.cs
```

## Paso 4: Agregar Referencias

### Referencias necesarias en el proyecto:
- ✅ System.Data.SQLite
- ✅ System.Windows.Forms
- ✅ System.Drawing
- ✅ Microsoft.VisualBasic (para InputBox)

Para agregar Microsoft.VisualBasic:
1. Click derecho en "Referencias" → "Agregar referencia"
2. Buscar `Microsoft.VisualBasic`
3. Marcar la casilla y click en "Aceptar"

## Paso 5: Configurar el Proyecto

### 5.1 Modificar Program.cs
```csharp
using System;
using System.Windows.Forms;
using SistemaPOS.Data;
using SistemaPOS.Forms;

namespace SistemaPOS
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Inicializar base de datos
            DatabaseHelper.InitializeDatabase();
            
            // Abrir formulario de login
            Application.Run(new LoginForm());
        }
    }
}
```

### 5.2 Copiar todos los archivos proporcionados
- Copia el contenido de cada archivo .cs a su ubicación correspondiente
- Asegúrate de que el namespace sea `SistemaPOS` en todos los archivos

## Paso 6: Compilar y Ejecutar

1. **Compilar**: Presiona `F6` o "Compilar" → "Compilar solución"
2. **Ejecutar**: Presiona `F5` o click en el botón ▶️ "Iniciar"

## Paso 7: Primera Ejecución

### Usuario por Defecto
- **Usuario**: `admin`
- **Contraseña**: `admin123`

⚠️ **IMPORTANTE**: Cambia esta contraseña inmediatamente después del primer inicio de sesión.

## Configuración del Hardware

### Escáner de Código de Barras
1. Conecta el escáner USB a la computadora
2. Windows lo detectará automáticamente
3. El escáner funcionará como un teclado
4. **No requiere configuración adicional** en la aplicación

### Impresora Térmica

#### Para Epson TM-T20 / TM-T88:
1. Instalar driver desde el sitio de Epson
2. Configurar como impresora predeterminada en Windows
3. Configurar puerto: USB o Red
4. Tamaño de papel: 80mm (estándar)

#### Integración con la aplicación:
El código actual muestra un preview del ticket. Para integración real:

```csharp
// En CobrarForm.cs, método ImprimirTicket:
private void ImprimirTicket(int idVenta, decimal montoRecibido)
{
    try
    {
        PrintDocument pd = new PrintDocument();
        pd.PrinterSettings.PrinterName = "TM-T20"; // Nombre de tu impresora
        pd.PrintPage += (sender, e) =>
        {
            // Aquí va el código de impresión
            Font font = new Font("Courier New", 10);
            float y = 10;
            
            e.Graphics.DrawString("SISTEMA POS", font, Brushes.Black, 10, y);
            y += 20;
            e.Graphics.DrawString($"Venta: {idVenta}", font, Brushes.Black, 10, y);
            // ... más líneas
        };
        pd.Print();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al imprimir: {ex.Message}");
    }
}
```

## Configuración Regional

### Cambiar Tasa de IVA
En `PuntoVentaForm.cs`, línea 18:
```csharp
private decimal tasaIVA = 0.16m; // Cambiar según tu país
// México: 0.16 (16%)
// Chile: 0.19 (19%)
// España: 0.21 (21%)
```

### Formato de Moneda
El sistema usa el formato regional de Windows automáticamente.
Para forzar un formato específico:

```csharp
// En la parte superior del archivo
using System.Globalization;

// Al inicio de tu Form
CultureInfo culture = new CultureInfo("es-MX"); // México
// CultureInfo culture = new CultureInfo("es-CL"); // Chile
// CultureInfo culture = new CultureInfo("es-ES"); // España
Thread.CurrentThread.CurrentCulture = culture;
Thread.CurrentThread.CurrentUICulture = culture;
```

## Resolución de Problemas Comunes

### Error: "System.Data.SQLite no encontrado"
**Solución**: Reinstalar el paquete NuGet
```
1. Herramientas → Administrador de paquetes NuGet → Consola
2. Ejecutar: Install-Package System.Data.SQLite.Core
```

### Error: "No se puede crear la base de datos"
**Solución**: Verificar permisos de escritura
- Ejecutar Visual Studio como Administrador
- O cambiar la ubicación del archivo .db a Mis Documentos

### La aplicación se ve borrosa en pantallas de alta resolución
**Solución**: Agregar al archivo `app.manifest`:
```xml
<application xmlns="urn:schemas-microsoft-com:asm.v3">
  <windowsSettings>
    <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
    <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
  </windowsSettings>
</application>
```

### El escáner no funciona
**Verificar**:
1. El escáner está en modo USB HID (modo teclado)
2. Configurar el escáner para enviar Enter después del código
3. Probar el escáner en el Bloc de notas de Windows

## Ubicación de la Base de Datos

Por defecto, la base de datos se crea en:
```
C:\[TuProyecto]\bin\Debug\pos_database.db
```

Para cambiar la ubicación, modificar en `DatabaseHelper.cs`:
```csharp
private static string dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
    "SistemaPOS",
    "pos_database.db"
);
```

## Backup Automático

Para configurar backups automáticos, agregar en `MainForm.cs`:
```csharp
private Timer backupTimer;

private void ConfigurarBackupAutomatico()
{
    backupTimer = new Timer();
    backupTimer.Interval = 86400000; // 24 horas
    backupTimer.Tick += (s, e) =>
    {
        string backupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SistemaPOS",
            "Backups",
            $"backup_{DateTime.Now:yyyyMMdd}.db"
        );
        DatabaseHelper.BackupDatabase(backupPath);
    };
    backupTimer.Start();
}
```

## Siguientes Pasos

1. ✅ Instalar y configurar el sistema
2. ✅ Cambiar la contraseña del administrador
3. ✅ Crear usuarios cajeros
4. ✅ Cargar categorías de productos
5. ✅ Cargar productos iniciales
6. ✅ Configurar impresora térmica
7. ✅ Probar una venta completa
8. ✅ Configurar backup automático

## Soporte y Ayuda

Para obtener los archivos faltantes mencionados en esta guía:
- `ProductoEditForm.cs`
- `AjustarStockForm.cs`
- `ReportesForm.cs`
- `UsuariosForm.cs`

Solicítalos en el siguiente mensaje y te los proporcionaré completos.

---

## Licencia y Uso

Este sistema es de código abierto y puede ser modificado libremente para adaptarlo a tus necesidades específicas.

**¡Sistema listo para usar! 🎉**