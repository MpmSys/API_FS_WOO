@echo off
REM ===============================================================================
REM  Script de Automatizacion para API_FS_WOO
REM  Este script ejecuta las tareas de sincronizacion en modo silencioso
REM ===============================================================================

setlocal enabledelayedexpansion

REM Configuracion
set APPPATH=%~dp0API_FS_WOO.exe
set LOGDIR=%~dp0Logs
set FECHA=%date:~-4%-%date:~3,2%-%date:~0,2%
set HORA=%time:~0,2%-%time:~3,2%-%time:~6,2%
set TIMESTAMP=%FECHA%_%HORA: =0%

REM Crear directorio de logs si no existe
if not exist "%LOGDIR%" (
    echo Creando directorio de logs: %LOGDIR%
    mkdir "%LOGDIR%"
)

echo.
echo ===============================================================================
echo    API_FS_WOO - Sincronizacion Automatica
echo    Fecha: %FECHA%  Hora: %time%
echo ===============================================================================
echo.

REM Menu de opciones
echo Seleccione la operacion a realizar:
echo.
echo   1. Descargar Stock (silencioso)
echo   2. Actualizar Pedidos (silencioso)
echo   3. Facturar Pedidos (silencioso)
echo   4. Todo (stock + pedidos + facturar)
echo   5. Descargar Stock (minimizado)
echo   6. Actualizar Pedidos (minimizado)
echo   7. Salir
echo.

set /p OPCION="Ingrese su opcion (1-7): "

if "%OPCION%"=="1" goto STOCK_SILENT
if "%OPCION%"=="2" goto PEDIDOS_SILENT
if "%OPCION%"=="3" goto FACTURAR_SILENT
if "%OPCION%"=="4" goto TODO
if "%OPCION%"=="5" goto STOCK_MIN
if "%OPCION%"=="6" goto PEDIDOS_MIN
if "%OPCION%"=="7" goto FIN
goto OPCION_INVALIDA

:STOCK_SILENT
echo.
echo [%time%] Descargando stock en modo silencioso...
"%APPPATH%" /silent /stock /log:"%LOGDIR%\stock_%TIMESTAMP%.log"
echo [%time%] Completado. Log: %LOGDIR%\stock_%TIMESTAMP%.log
goto FIN

:PEDIDOS_SILENT
echo.
echo [%time%] Actualizando pedidos en modo silencioso...
"%APPPATH%" /silent /actualizar /log:"%LOGDIR%\pedidos_%TIMESTAMP%.log"
echo [%time%] Completado. Log: %LOGDIR%\pedidos_%TIMESTAMP%.log
goto FIN

:FACTURAR_SILENT
echo.
echo [%time%] Facturando pedidos en modo silencioso...
"%APPPATH%" /silent /facturar /log:"%LOGDIR%\facturas_%TIMESTAMP%.log"
echo [%time%] Completado. Log: %LOGDIR%\facturas_%TIMESTAMP%.log
goto FIN

:TODO
echo.
echo [%time%] Ejecutando sincronizacion completa...
echo.
echo [%time%] 1/3 - Descargando stock...
"%APPPATH%" /silent /stock /log:"%LOGDIR%\stock_%TIMESTAMP%.log"
timeout /t 5 /nobreak > nul

echo [%time%] 2/3 - Actualizando pedidos...
"%APPPATH%" /silent /actualizar /log:"%LOGDIR%\pedidos_%TIMESTAMP%.log"
timeout /t 5 /nobreak > nul

echo [%time%] 3/3 - Facturando pedidos...
"%APPPATH%" /silent /facturar /log:"%LOGDIR%\facturas_%TIMESTAMP%.log"

echo.
echo [%time%] Sincronizacion completa terminada.
echo Logs guardados en: %LOGDIR%
goto FIN

:STOCK_MIN
echo.
echo [%time%] Descargando stock en modo minimizado...
"%APPPATH%" /minimized /stock
goto FIN

:PEDIDOS_MIN
echo.
echo [%time%] Actualizando pedidos en modo minimizado...
"%APPPATH%" /minimized /actualizar
goto FIN

:OPCION_INVALIDA
echo.
echo ERROR: Opcion no valida. Por favor seleccione una opcion entre 1 y 7.
timeout /t 3 /nobreak > nul
goto INICIO

:FIN
echo.
echo ===============================================================================
echo    Proceso finalizado
echo ===============================================================================
echo.
pause
endlocal
