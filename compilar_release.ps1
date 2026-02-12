# Script de Compilación Release - Sistema POS
# Modo Release optimizado para distribución

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Sistema POS - Compilación Release" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Limpiar proyecto
Write-Host "[1/3] Limpiando proyecto..." -ForegroundColor Yellow
msbuild SistemaPOS.csproj /t:Clean /nologo /v:quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al limpiar el proyecto" -ForegroundColor Red
    exit 1
}
Write-Host "      Limpieza completada" -ForegroundColor Green

# Paso 2: Restaurar paquetes NuGet
Write-Host "[2/3] Restaurando paquetes NuGet..." -ForegroundColor Yellow
nuget restore SistemaPOS.csproj -NonInteractive 2>$null

Write-Host "      Paquetes restaurados" -ForegroundColor Green

# Paso 3: Compilar en modo Release
Write-Host "[3/3] Compilando en modo Release..." -ForegroundColor Yellow
msbuild SistemaPOS.csproj /p:Configuration=Release /v:minimal /nologo /m

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Fallo en la compilación" -ForegroundColor Red
    Write-Host "Revisa los errores arriba para más detalles" -ForegroundColor Red
    exit 1
}

Write-Host "      Compilación Release exitosa!" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "   COMPILACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Ejecutable generado en:" -ForegroundColor Cyan
Write-Host "  .\bin\Release\SistemaPOS.exe" -ForegroundColor White
Write-Host ""
Write-Host "Archivos listos para distribución en:" -ForegroundColor Cyan
Write-Host "  .\bin\Release\" -ForegroundColor White
Write-Host ""

# Opcional: Mostrar tamaño del ejecutable
$exePath = ".\bin\Release\SistemaPOS.exe"
if (Test-Path $exePath) {
    $fileSize = (Get-Item $exePath).Length / 1KB
    Write-Host "Tamaño del ejecutable: $($fileSize.ToString('N2')) KB" -ForegroundColor Gray
}

Write-Host ""
Write-Host "¿Deseas ejecutar la versión Release? (S/N)" -ForegroundColor Yellow
$respuesta = Read-Host

if ($respuesta -eq "S" -or $respuesta -eq "s") {
    Write-Host "Ejecutando..." -ForegroundColor Green
    .\bin\Release\SistemaPOS.exe
}
