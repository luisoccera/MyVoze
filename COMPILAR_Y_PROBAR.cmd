@echo off
setlocal
cd /d "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo No se encontro .NET SDK 10.
    echo Instala .NET SDK 10 y vuelve a ejecutar este archivo.
    pause
    exit /b 1
)

dotnet restore ".\VoxMorph.sln"
if errorlevel 1 goto :error

dotnet build ".\VoxMorph.sln" -c Release --no-restore
if errorlevel 1 goto :error

dotnet test ".\VoxMorph.sln" -c Release --no-build
if errorlevel 1 goto :error

echo.
echo VoxMorph compilo y las pruebas terminaron correctamente.
pause
exit /b 0

:error
echo.
echo Se encontro un error. Copia el mensaje completo para revisarlo.
pause
exit /b 1
