@echo off
setlocal
cd /d "%~dp0"

if not exist ".\Publicacion\win-x64\MyVoze.exe" (
    echo No se encontro .\Publicacion\win-x64\MyVoze.exe
    echo Vuelve a copiar la carpeta completa de MyVoze.
    pause
    exit /b 1
)

start "" ".\Publicacion\win-x64\MyVoze.exe"
if errorlevel 1 (
    echo.
    echo MyVoze no pudo iniciar. Revisa el mensaje mostrado arriba.
    pause
)
