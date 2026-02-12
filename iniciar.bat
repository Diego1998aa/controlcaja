@echo off
echo ========================================
echo   SISTEMA POS - INICIADOR AUTOMATICO
echo ========================================

REM 1. Buscar compilador C# (CSC)
set "CSC="

REM Prioridad 1: Visual Studio 2022 Build Tools (Roslyn)
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe" set "CSC=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
if defined CSC goto FOUND_CSC

REM Prioridad 2: Visual Studio 2022 Community/Enterprise (Roslyn)
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe" set "CSC=C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
if defined CSC goto FOUND_CSC

if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe" set "CSC=C:\Program Files (x86)\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe"
if defined CSC goto FOUND_CSC

REM Prioridad 3: .NET Framework (C# 5.0 - Fallback, puede fallar con codigo moderno)
if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if defined CSC goto FOUND_CSC
if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" set "CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"

:FOUND_CSC
if defined CSC goto PREPARE_DIR
echo ERROR: No se encontro el compilador de C# (csc.exe).
echo Por favor instala Visual Studio Build Tools o .NET Framework.
pause
exit /b 1

:PREPARE_DIR
echo Usando compilador: %CSC%
if not exist "bin" mkdir "bin"
if not exist "bin\x86" mkdir "bin\x86"
if not exist "bin\x64" mkdir "bin\x64"

REM 3. Copiar dependencias (SQLite)
echo Copiando librerias...

set "SQLITE_PKG=packages\Stub.System.Data.SQLite.Core.NetFramework.1.0.118.0"
set "SQLITE_DLL=%SQLITE_PKG%\lib\net46\System.Data.SQLite.dll"

if exist "%SQLITE_DLL%" goto COPY_DLL
echo ERROR: No se encuentra System.Data.SQLite.dll en packages.
echo Ruta buscada: %SQLITE_DLL%
pause
exit /b 1

:COPY_DLL
copy /Y "%SQLITE_DLL%" "bin\" >nul
copy /Y "%SQLITE_PKG%\build\net46\x86\SQLite.Interop.dll" "bin\x86\" >nul
copy /Y "%SQLITE_PKG%\build\net46\x64\SQLite.Interop.dll" "bin\x64\" >nul
copy /Y "%SQLITE_PKG%\build\net46\x64\SQLite.Interop.dll" "bin\" >nul

REM 4. Compilar
echo Compilando aplicacion...

"%CSC%" /target:winexe /out:bin\SistemaPOS.exe /r:bin\System.Data.SQLite.dll /r:System.dll /r:System.Core.dll /r:System.Data.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Xml.dll Program.cs Data\*.cs Forms\*.cs Helpers\*.cs Models\*.cs Services\*.cs

if %ERRORLEVEL% NEQ 0 goto ERROR_COMPILE

echo.
echo Compilacion exitosa. Iniciando Sistema POS...
echo.

REM 5. Ejecutar
start "" "bin\SistemaPOS.exe"
exit /b 0

:ERROR_COMPILE
echo.
echo ERROR DE COMPILACION
pause
exit /b 1
