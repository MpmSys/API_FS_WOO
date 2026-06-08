# Script de prueba para verificar el modo silencioso con log
# Ejecutar desde PowerShell en: C:\Users\manel\source\repos\MpmSys\API_FS_WOO\API_FS_WOO
# Comando: .\test_actualizar.ps1

# Configuración
$exePath = ".\bin\Debug\API_FS_WOO.exe"
$logPath = "C:\Logs\actualizar.txt"

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  PRUEBA DE API_FS_WOO - MODO SILENCIOSO CON LOG" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Verificar que estamos en el directorio correcto
$expectedPath = "API_FS_WOO"
$currentDir = Split-Path -Leaf (Get-Location)
if ($currentDir -ne $expectedPath) {
    Write-Host "⚠ ADVERTENCIA: No estás en el directorio correcto" -ForegroundColor Yellow
    Write-Host "  Directorio actual: $(Get-Location)" -ForegroundColor Yellow
    Write-Host "  Directorio esperado: ...\API_FS_WOO\API_FS_WOO" -ForegroundColor Yellow
    Write-Host ""
}

# Crear directorio de logs si no existe
$logDir = Split-Path $logPath -Parent
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    Write-Host "✓ Directorio de logs creado: $logDir" -ForegroundColor Green
} else {
    Write-Host "✓ Directorio de logs existe: $logDir" -ForegroundColor Green
}

# Eliminar log anterior si existe
if (Test-Path $logPath) {
    Remove-Item $logPath -Force
    Write-Host "✓ Log anterior eliminado" -ForegroundColor Yellow
}

# Verificar que el ejecutable existe
if (-not (Test-Path $exePath)) {
    Write-Host ""
    Write-Host "✗ ERROR: No se encuentra el ejecutable" -ForegroundColor Red
    Write-Host "  Ruta buscada: $exePath" -ForegroundColor Red
    Write-Host "  Ruta completa: $((Get-Location).Path)\$exePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Por favor, compila el proyecto primero:" -ForegroundColor Yellow
    Write-Host "  1. Abre el proyecto en Visual Studio" -ForegroundColor White
    Write-Host "  2. Presiona F5 o Ctrl+Shift+B para compilar" -ForegroundColor White
    Write-Host ""
    pause
    exit 1
}

Write-Host "✓ Ejecutable encontrado: $exePath" -ForegroundColor Green
Write-Host ""
Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host "Ejecutando: $exePath" -ForegroundColor White
Write-Host "Parámetros: /silent /actualizar /log:`"$logPath`"" -ForegroundColor White
Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# Iniciar monitoreo del log en segundo plano
$monitorJob = Start-Job -ScriptBlock {
    param($path)
    $timeout = 30
    $elapsed = 0
    while (-not (Test-Path $path) -and $elapsed -lt $timeout) {
        Start-Sleep -Milliseconds 100
        $elapsed++
    }

    if (Test-Path $path) {
        Get-Content $path -Wait
    }
} -ArgumentList $logPath

# Ejecutar el programa
$process = Start-Process -FilePath $exePath -ArgumentList '/silent', '/actualizar', "/log:`"$logPath`"" -PassThru -WindowStyle Hidden

Write-Host "✓ Proceso iniciado (PID: $($process.Id))" -ForegroundColor Green
Write-Host "⏳ Monitoreando log en tiempo real..." -ForegroundColor Yellow
Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CONTENIDO DEL LOG (TIEMPO REAL)" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Mostrar el log en tiempo real
$startTime = Get-Date
while (-not $process.HasExited -and ((Get-Date) - $startTime).TotalSeconds -lt 300) {
    $logData = Receive-Job -Job $monitorJob
    if ($logData) {
        foreach ($line in $logData) {
            Write-Host $line -ForegroundColor Cyan
        }
    }
    Start-Sleep -Milliseconds 500
}

Stop-Job -Job $monitorJob
Remove-Job -Job $monitorJob

Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan

if ($process.HasExited) {
    Write-Host ""
    Write-Host "✓ Proceso finalizado (Código: $($process.ExitCode))" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "⚠ El proceso sigue ejecutándose después de 5 minutos" -ForegroundColor Yellow
    $process.Kill()
    Write-Host "✗ Proceso terminado forzadamente" -ForegroundColor Red
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  VERIFICACIÓN DE RESULTADOS" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $logPath) {
    Write-Host "✓ Archivo de log creado exitosamente" -ForegroundColor Green
    Write-Host "  Ubicación: $logPath" -ForegroundColor White

    $fileInfo = Get-Item $logPath
    Write-Host ""
    Write-Host "Información del archivo:" -ForegroundColor Cyan
    Write-Host "  Tamaño: $($fileInfo.Length) bytes" -ForegroundColor White
    Write-Host "  Creado: $($fileInfo.CreationTime)" -ForegroundColor White
    Write-Host "  Modificado: $($fileInfo.LastWriteTime)" -ForegroundColor White

    Write-Host ""
    Write-Host "Contenido completo:" -ForegroundColor Cyan
    Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor Gray
    Get-Content $logPath | ForEach-Object { Write-Host $_ -ForegroundColor White }
    Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor Gray
} else {
    Write-Host "✗ ERROR: No se creó el archivo de log" -ForegroundColor Red
    Write-Host "  Ruta esperada: $logPath" -ForegroundColor Yellow

    $desktopLog = Join-Path $env:USERPROFILE "Desktop\API_FS_WOO_Log.txt"
    if (Test-Path $desktopLog) {
        Write-Host ""
        Write-Host "✓ Log alternativo en el escritorio" -ForegroundColor Yellow
        Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor Gray
        Get-Content $desktopLog | ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
        Write-Host "────────────────────────────────────────────────────────────────" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  PRUEBA COMPLETADA" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
