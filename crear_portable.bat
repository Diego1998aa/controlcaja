@echo off
echo ==================================================
echo   CREANDO VERSION PORTABLE DE SISTEMA POS
echo ==================================================

REM 1. Configuración de rutas
set "SOURCE_DIR=%~dp0"
set "DIST_DIR=%SOURCE_DIR%SistemaPOS_Portable"
set "BIN_DIR=%SOURCE_DIR%bin"

REM 2. Limpiar compilación anterior
if exist "%DIST_DIR%" rmdir /s /q "%DIST_DIR%"
mkdir "%DIST_DIR%"

REM 3. Buscar compilador C# (CSC)
set "CSC="
REM Prioridad 1: Visual Studio 2022 Build Tools (Roslyn)
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe" set "CSC=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
if defined CSC goto FOUND_CSC
REM Prioridad 2: Visual Studio 2022 Community/Enterprise (Roslyn)
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe" set "CSC=C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
if defined CSC goto FOUND_CSC
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe" set "CSC=C:\Program Files (x86)\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe"
if defined CSC goto FOUND_CSC
REM Fallback .NET Framework
if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if defined CSC goto FOUND_CSC

echo ERROR: No se encontro el compilador csc.exe
pause
exit /b 1

:FOUND_CSC
echo Usando compilador: %CSC%

REM 4. Preparar estructura de dependencias
if not exist "%DIST_DIR%\x86" mkdir "%DIST_DIR%\x86"
if not exist "%DIST_DIR%\x64" mkdir "%DIST_DIR%\x64"

set "SQLITE_PKG=packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.118.0"
set "SQLITE_DLL=%SQLITE_PKG%\lib\net46\System.Data.SQLite.dll"

echo Copiando librerias SQLite...
copy /Y "%SQLITE_DLL%" "%DIST_DIR%\" >nul
copy /Y "%SQLITE_PKG%\build\net46\x86\SQLite.Interop.dll" "%DIST_DIR%\x86\" >nul
copy /Y "%SQLITE_PKG%\build\net46\x64\SQLite.Interop.dll" "%DIST_DIR%\x64\" >nul

REM 5. Compilar
echo Compilando ejecutable...
"%CSC%" /target:winexe /out:"%DIST_DIR%\SistemaPOS.exe" /r:"%DIST_DIR%\System.Data.SQLite.dll" /r:System.dll /r:System.Core.dll /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Xml.dll Program.cs Data\*.cs Forms\*.cs Helpers\*.cs Models\*.cs Services\*.cs

if %ERRORLEVEL% NEQ 0 (
    echo ERROR DE COMPILACION
    pause
    exit /b 1
)

REM 6. Crear instrucciones
echo Creando LEEME.txt...
(
echo ==========================================
echo   SISTEMA POS - VERSION PORTABLE
echo ==========================================
echo.
echo Esta carpeta contiene todo lo necesario para ejecutar el Sistema POS.
echo No requiere instalacion de librerias adicionales.
echo.
echo REQUISITOS:
echo - Windows 7, 8, 10 u 11
echo - .NET Framework 4.7.2 o superior ^(viene instalado por defecto en Windows 10/11^)
echo.
echo INSTRUCCIONES:
echo 1. Copie esta carpeta completa al PC de destino.
echo 2. Ejecute "SistemaPOS.exe".
echo.
echo DATOS:
echo La base de datos se creara automaticamente en:
echo %%AppData%%\SistemaPOS\pos_database.db
echo.
echo Si desea reiniciar la base de datos, puede borrar ese archivo.
) > "%DIST_DIR%\LEEME.txt"

echo.
echo ==================================================
echo   VERSION PORTABLE CREADA EXITOSAMENTE
echo ==================================================
echo Ubicacion: %DIST_DIR%
echo.
pause
