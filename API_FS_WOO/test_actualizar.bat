@echo off
REM Script de prueba rápida para modo silencioso
REM Ejecutar desde CMD como Administrador
REM Desde: C:\Users\manel\source\repos\MpmSys\API_FS_WOO\API_FS_WOO

echo ========================================
echo Prueba de API_FS_WOO en modo silencioso
echo ========================================
echo.

REM Crear directorio de logs si no existe
if not exist "C:\Logs\" (
    echo Creando directorio C:\Logs...
    mkdir "C:\Logs"
)

REM Eliminar log anterior
if exist "C:\Logs\actualizar.txt" (
    echo Eliminando log anterior...
    del "C:\Logs\actualizar.txt"
)

REM Ruta al ejecutable
set EXE_PATH=.\bin\Debug\API_FS_WOO.exe

REM Verificar que existe el ejecutable
if not exist "%EXE_PATH%" (
    echo ERROR: No se encuentra el ejecutable en: %EXE_PATH%
    echo Ruta completa: %CD%\%EXE_PATH%
    echo.
    echo Por favor, compila el proyecto primero.
    echo   1. Abre el proyecto en Visual Studio
    echo   2. Presiona F5 o Ctrl+Shift+B
    echo.
    pause
    exit /b 1
)

echo Ejecutable encontrado: %EXE_PATH%
echo.
echo Ejecutando con parametros:
echo   /silent /actualizar /log:"C:\Logs\actualizar.txt"
echo.
echo Esperando a que termine...
echo.

REM Ejecutar el programa y esperar
"%EXE_PATH%" /silent /actualizar /log:"C:\Logs\actualizar.txt"

REM Esperar un poco más
timeout /t 2 /nobreak >nul

echo.
echo ========================================
echo Verificando resultados...
echo ========================================
echo.

REM Verificar si se creó el log
if exist "C:\Logs\actualizar.txt" (
    echo [OK] Archivo de log creado exitosamente
    echo.
    echo Contenido del log:
    echo ----------------------------------------
    type "C:\Logs\actualizar.txt"
    echo ----------------------------------------
    echo.

    REM Mostrar información del archivo
    for %%A in ("C:\Logs\actualizar.txt") do (
        echo Tamano: %%~zA bytes
        echo Fecha: %%~tA
    )
) else (
    echo [ERROR] No se creo el archivo de log
    echo.

    REM Buscar log en escritorio
    if exist "%USERPROFILE%\Desktop\API_FS_WOO_Log.txt" (
        echo [ATENCION] Se encontro un log alternativo en el escritorio
        echo.
        echo Contenido:
        echo ----------------------------------------
        type "%USERPROFILE%\Desktop\API_FS_WOO_Log.txt"
        echo ----------------------------------------
    ) else (
        echo No se encontro log alternativo en el escritorio
        echo.
        echo Posibles causas:
        echo   - Falta de permisos en C:\Logs
        echo   - El programa fallo antes de escribir
        echo   - Problema con la ruta del ejecutable
    )
)

echo.
echo Prueba completada.
echo.
pause
