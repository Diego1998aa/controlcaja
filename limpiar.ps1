# Script de Limpieza - Sistema POS
# Elimina archivos de compilación y temporales

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Sistema POS - Limpieza de Archivos" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Este script eliminará:" -ForegroundColor Yellow
Write-Host "  - Carpeta bin\" -ForegroundColor Gray
Write-Host "  - Carpeta obj\" -ForegroundColor Gray
Write-Host "  - Archivos temporales de compilación" -ForegroundColor Gray
Write-Host ""

Write-Host "¿Estás seguro de continuar? (S/N)" -ForegroundColor Yellow
$respuesta = Read-Host

if ($respuesta -ne "S" -and $respuesta -ne "s") {
    Write-Host "Operación cancelada." -ForegroundColor Gray
    exit 0
}

Write-Host ""
Write-Host "Limpiando archivos..." -ForegroundColor Yellow

# Eliminar carpetas bin y obj
$foldersToDelete = @("bin", "obj")
$deletedCount = 0

foreach ($folder in $foldersToDelete) {
    if (Test-Path $folder) {
        Write-Host "  Eliminando $folder\..." -ForegroundColor Gray
        Remove-Item -Recurse -Force $folder -ErrorAction SilentlyContinue
        $deletedCount++
    }
}

# Limpiar con MSBuild
Write-Host "  Ejecutando limpieza de MSBuild..." -ForegroundColor Gray
msbuild SistemaPOS.csproj /t:Clean /nologo /v:quiet 2>$null

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "   LIMPIEZA COMPLETADA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Carpetas eliminadas: $deletedCount" -ForegroundColor Cyan
Write-Host ""
Write-Host "El proyecto está listo para una compilación limpia." -ForegroundColor Gray
Write-Host "Ejecuta '.\compilar.ps1' para compilar nuevamente." -ForegroundColor Gray
Write-Host ""
