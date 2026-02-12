# 🚀 Guía Rápida de Compilación

## ⚡ Uso Rápido (Scripts PowerShell)

### Compilar y Ejecutar (Debug)
```powershell
.\compilar.ps1
```
Este script:
- ✅ Limpia el proyecto
- ✅ Restaura paquetes NuGet
- ✅ Compila en modo Debug
- ✅ Ejecuta automáticamente la aplicación

### Compilar para Distribución (Release)
```powershell
.\compilar_release.ps1
```
Este script:
- ✅ Limpia el proyecto
- ✅ Restaura paquetes NuGet
- ✅ Compila en modo Release (optimizado)
- ✅ Te pregunta si quieres ejecutar

### Limpiar Archivos de Compilación
```powershell
.\limpiar.ps1
```
Este script elimina:
- 🗑️ Carpeta `bin\`
- 🗑️ Carpeta `obj\`
- 🗑️ Archivos temporales

---

## 📝 Comandos Manuales

### Compilar desde Cero
```bash
msbuild SistemaPOS.csproj /t:Clean;Build /p:Configuration=Debug
```

### Solo Compilar
```bash
msbuild SistemaPOS.csproj /p:Configuration=Debug
```

### Ejecutar después de compilar
```bash
.\bin\Debug\SistemaPOS.exe
```

---

## 🔧 Primer Uso

1. **Abre PowerShell** en la carpeta del proyecto:
   ```powershell
   cd C:\Users\Usuario\Desktop\SistemaPOS
   ```

2. **Habilita la ejecución de scripts** (solo la primera vez):
   ```powershell
   Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
   ```

3. **Compila y ejecuta**:
   ```powershell
   .\compilar.ps1
   ```

---

## 📚 Documentación Completa

Para ver todos los comandos disponibles y solución de problemas:
- 📖 [COMANDOS_TERMINAL.md](COMANDOS_TERMINAL.md)

---

## ⚠️ Solución Rápida de Problemas

### "No se puede ejecutar scripts"
```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

### "MSBuild no se reconoce"
Usa la ruta completa de MSBuild:
```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" SistemaPOS.csproj
```

### Error de paquetes NuGet
```powershell
nuget restore SistemaPOS.csproj
```

---

## 🎯 Workflow Diario Recomendado

**Para desarrollo:**
```powershell
.\compilar.ps1
```

**Antes de distribuir:**
```powershell
.\compilar_release.ps1
```

**Para limpiar:**
```powershell
.\limpiar.ps1
```

---

## ✨ Mejoras Implementadas

El sistema ahora incluye:
- ✅ Sistema de permisos robusto (Vendedor, Cajera, Supervisor)
- ✅ Validaciones de seguridad en usuarios y productos
- ✅ ProductoEditForm completo (SKU, Descripción, Stock Mínimo)
- ✅ Alertas visuales de stock (rojo/naranja)
- ✅ Interfaz profesional sin emojis
- ✅ Migración automática de roles antiguos

---

¡Listo para compilar! 🎉
