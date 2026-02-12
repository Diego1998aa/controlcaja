# Guía de Comandos de Terminal - Sistema POS

## 📋 Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:
- **.NET Framework 4.7.2 o superior** (viene con Visual Studio)
- **MSBuild** (incluido con Visual Studio o .NET SDK)
- **NuGet** (gestor de paquetes)

---

## 🚀 Comandos Básicos

### 1. Navegar al Directorio del Proyecto

```bash
cd C:\Users\Usuario\Desktop\SistemaPOS
```

---

## 🔨 Compilación del Proyecto

### Compilar el proyecto (Modo Debug)

```bash
msbuild SistemaPOS.csproj /p:Configuration=Debug
```

### Compilar el proyecto (Modo Release - Optimizado)

```bash
msbuild SistemaPOS.csproj /p:Configuration=Release
```

### Compilar y mostrar información detallada

```bash
msbuild SistemaPOS.csproj /p:Configuration=Debug /v:detailed
```

### Limpiar archivos de compilación

```bash
msbuild SistemaPOS.csproj /t:Clean
```

### Limpiar y compilar de nuevo

```bash
msbuild SistemaPOS.csproj /t:Clean;Build /p:Configuration=Debug
```

---

## ▶️ Ejecutar el Proyecto

### Ejecutar después de compilar (Debug)

```bash
.\bin\Debug\SistemaPOS.exe
```

### Ejecutar después de compilar (Release)

```bash
.\bin\Release\SistemaPOS.exe
```

### Compilar y ejecutar en un solo comando (PowerShell)

```powershell
msbuild SistemaPOS.csproj /p:Configuration=Debug; if ($?) { .\bin\Debug\SistemaPOS.exe }
```

---

## 📦 Gestión de Paquetes NuGet

### Restaurar paquetes NuGet

```bash
nuget restore SistemaPOS.csproj
```

### Actualizar todos los paquetes

```bash
nuget update SistemaPOS.csproj
```

### Instalar un paquete específico

```bash
nuget install System.Data.SQLite -OutputDirectory packages
```

---

## 🗂️ Estructura de Carpetas de Compilación

Después de compilar, encontrarás los archivos en:

```
SistemaPOS/
├── bin/
│   ├── Debug/              # Compilación en modo Debug
│   │   ├── SistemaPOS.exe  # Ejecutable
│   │   ├── SistemaPOS.pdb  # Símbolos de depuración
│   │   └── *.dll           # Librerías dependientes
│   └── Release/            # Compilación en modo Release (optimizada)
│       └── SistemaPOS.exe
└── obj/                    # Archivos temporales de compilación
```

---

## 🔧 Comandos Útiles Adicionales

### Ver información del proyecto

```bash
msbuild SistemaPOS.csproj /t:GetTargetPath
```

### Compilar solo si hay cambios

```bash
msbuild SistemaPOS.csproj /p:Configuration=Debug /maxcpucount
```

### Compilar con múltiples procesadores (más rápido)

```bash
msbuild SistemaPOS.csproj /p:Configuration=Debug /m
```

### Generar archivo de log de compilación

```bash
msbuild SistemaPOS.csproj /p:Configuration=Debug /flp:logfile=build.log;verbosity=detailed
```

---

## 🐛 Depuración desde Terminal

### Compilar en modo Debug con símbolos completos

```bash
msbuild SistemaPOS.csproj /p:Configuration=Debug /p:DebugType=full
```

### Ver advertencias detalladas

```bash
msbuild SistemaPOS.csproj /p:Configuration=Debug /v:detailed /clp:WarningsOnly
```

---

## 🧹 Limpieza de Archivos

### Eliminar carpetas bin y obj

```powershell
# PowerShell
Remove-Item -Recurse -Force bin, obj
```

```cmd
# CMD
rmdir /s /q bin
rmdir /s /q obj
```

### Limpiar y eliminar paquetes NuGet descargados

```powershell
# PowerShell
Remove-Item -Recurse -Force bin, obj, packages
```

---

## 📋 Scripts Útiles

### Script de Compilación Rápida (PowerShell)

Crea un archivo `compilar.ps1`:

```powershell
# compilar.ps1
Write-Host "Limpiando proyecto..." -ForegroundColor Yellow
msbuild SistemaPOS.csproj /t:Clean /nologo

Write-Host "Restaurando paquetes NuGet..." -ForegroundColor Yellow
nuget restore SistemaPOS.csproj

Write-Host "Compilando proyecto..." -ForegroundColor Yellow
msbuild SistemaPOS.csproj /p:Configuration=Debug /v:minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "Compilación exitosa!" -ForegroundColor Green
    Write-Host "Ejecutando aplicación..." -ForegroundColor Green
    .\bin\Debug\SistemaPOS.exe
} else {
    Write-Host "Error en la compilación" -ForegroundColor Red
}
```

Ejecutar:
```powershell
.\compilar.ps1
```

### Script de Compilación Release (PowerShell)

Crea un archivo `compilar_release.ps1`:

```powershell
# compilar_release.ps1
Write-Host "Compilando en modo Release..." -ForegroundColor Yellow
msbuild SistemaPOS.csproj /t:Clean;Build /p:Configuration=Release /v:minimal

if ($LASTEXITCODE -eq 0) {
    Write-Host "Compilación Release exitosa!" -ForegroundColor Green
    Write-Host "Ejecutable: .\bin\Release\SistemaPOS.exe" -ForegroundColor Cyan
}
```

---

## 🔍 Solución de Problemas Comunes

### Error: "MSBuild no se reconoce como comando"

**Solución:** Agrega MSBuild al PATH o usa la ruta completa:

```powershell
# Ruta típica de MSBuild
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" SistemaPOS.csproj
```

O agrega al PATH:
```powershell
$env:Path += ";C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin"
```

### Error: "Paquetes NuGet no encontrados"

**Solución:** Restaura los paquetes primero:

```bash
nuget restore SistemaPOS.csproj
msbuild SistemaPOS.csproj /p:Configuration=Debug
```

### Error: "No se puede encontrar System.Data.SQLite"

**Solución:** Instala el paquete manualmente:

```bash
nuget install System.Data.SQLite -OutputDirectory packages -Version 1.0.118
```

### Error de compilación por archivos bloqueados

**Solución:** Cierra todas las instancias de SistemaPOS.exe y limpia:

```powershell
# Matar proceso si está corriendo
taskkill /F /IM SistemaPOS.exe 2>$null

# Limpiar y compilar
msbuild SistemaPOS.csproj /t:Clean;Build /p:Configuration=Debug
```

---

## 🎯 Workflow Recomendado

### Para Desarrollo Diario:

```bash
# 1. Navegar al proyecto
cd C:\Users\Usuario\Desktop\SistemaPOS

# 2. Limpiar compilación anterior
msbuild SistemaPOS.csproj /t:Clean /nologo

# 3. Compilar en modo Debug
msbuild SistemaPOS.csproj /p:Configuration=Debug /v:minimal

# 4. Ejecutar
.\bin\Debug\SistemaPOS.exe
```

### Para Producción/Distribución:

```bash
# 1. Navegar al proyecto
cd C:\Users\Usuario\Desktop\SistemaPOS

# 2. Limpiar todo
msbuild SistemaPOS.csproj /t:Clean

# 3. Compilar en modo Release
msbuild SistemaPOS.csproj /p:Configuration=Release /v:minimal

# 4. El ejecutable estará en:
# .\bin\Release\SistemaPOS.exe
```

---

## 📝 Notas Importantes

1. **Modo Debug vs Release:**
   - **Debug:** Incluye símbolos de depuración, sin optimizaciones
   - **Release:** Optimizado, sin símbolos de depuración, más rápido

2. **Base de datos:**
   - La base de datos se crea automáticamente en `Data\pos_database.db`
   - Si migras de roles antiguos, se mostrará un mensaje al iniciar

3. **Permisos:**
   - El sistema ahora usa 3 roles: Vendedor, Cajera, Supervisor
   - Los usuarios antiguos con rol "Cajero" se migrarán automáticamente a "Cajera"

4. **Archivos necesarios:**
   - Asegúrate de que `System.Data.SQLite.dll` esté en la carpeta bin después de compilar
   - Los archivos de configuración se generan automáticamente

---

## 🚀 Atajos Rápidos (PowerShell)

Puedes crear aliases en tu perfil de PowerShell (`$PROFILE`):

```powershell
# Agregar al archivo de perfil de PowerShell
function Compilar-POS {
    cd C:\Users\Usuario\Desktop\SistemaPOS
    msbuild SistemaPOS.csproj /p:Configuration=Debug /v:minimal
}

function Ejecutar-POS {
    cd C:\Users\Usuario\Desktop\SistemaPOS
    .\bin\Debug\SistemaPOS.exe
}

function Compilar-Ejecutar-POS {
    cd C:\Users\Usuario\Desktop\SistemaPOS
    msbuild SistemaPOS.csproj /t:Clean;Build /p:Configuration=Debug /v:minimal
    if ($LASTEXITCODE -eq 0) { .\bin\Debug\SistemaPOS.exe }
}

# Alias cortos
Set-Alias cpos Compilar-POS
Set-Alias epos Ejecutar-POS
Set-Alias cepos Compilar-Ejecutar-POS
```

Luego solo escribe:
```powershell
cpos    # Compilar
epos    # Ejecutar
cepos   # Compilar y ejecutar
```

---

## 📚 Recursos Adicionales

- **Documentación MSBuild:** https://docs.microsoft.com/es-es/visualstudio/msbuild/
- **NuGet CLI Reference:** https://docs.microsoft.com/es-es/nuget/reference/nuget-exe-cli-reference
- **System.Data.SQLite:** https://system.data.sqlite.org/

---

¡Listo! Ahora puedes compilar y ejecutar tu Sistema POS completamente desde la terminal. 🎉
