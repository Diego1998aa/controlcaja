# Script de Compilación Rápida - Sistema POS
# Modo Debug con ejecución automática

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Sistema POS - Compilación Debug" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Limpiar proyecto
Write-Host "[1/4] Limpiando proyecto..." -ForegroundColor Yellow
msbuild SistemaPOS.csproj /t:Clean /nologo /v:quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al limpiar el proyecto" -ForegroundColor Red
    exit 1
}
Write-Host "      Limpieza completada" -ForegroundColor Green

# Paso 2: Restaurar paquetes NuGet
Write-Host "[2/4] Restaurando paquetes NuGet..." -ForegroundColor Yellow
nuget restore SistemaPOS.csproj -NonInteractive 2>$null

Write-Host "      Paquetes restaurados" -ForegroundColor Green

# Paso 3: Compilar proyecto
Write-Host "[3/4] Compilando proyecto..." -ForegroundColor Yellow
msbuild SistemaPOS.csproj /p:Configuration=Debug /v:minimal /nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Fallo en la compilación" -ForegroundColor Red
    Write-Host "Revisa los errores arriba para más detalles" -ForegroundColor Red
    exit 1
}

Write-Host "      Compilación exitosa!" -ForegroundColor Green

# Paso 4: Ejecutar aplicación
Write-Host "[4/4] Ejecutando aplicación..." -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Iniciando Sistema POS..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Start-Sleep -Milliseconds 500
.\bin\Debug\SistemaPOS.exe

Write-Host ""
Write-Host "Aplicación cerrada." -ForegroundColor Gray
