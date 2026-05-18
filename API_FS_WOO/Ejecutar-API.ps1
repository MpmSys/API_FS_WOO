<#
.SYNOPSIS
    Script de automatización para API_FS_WOO

.DESCRIPTION
    Ejecuta las operaciones de sincronización de API_FS_WOO en modo silencioso
    o minimizado, con soporte para logs y notificaciones por email.

.PARAMETER Comando
    Comando a ejecutar: stock, actualizar, facturar, todo

.PARAMETER Modo
    Modo de ejecución: silent (predeterminado), minimized

.PARAMETER LogDir
    Directorio donde guardar los logs

.PARAMETER EnviarEmail
    Enviar notificación por email al completar

.PARAMETER EmailDestino
    Dirección de email para notificaciones

.EXAMPLE
    .\Ejecutar-API.ps1 -Comando stock

.EXAMPLE
    .\Ejecutar-API.ps1 -Comando todo -EnviarEmail -EmailDestino "admin@empresa.com"

.EXAMPLE
    .\Ejecutar-API.ps1 -Comando actualizar -Modo minimized
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("stock", "actualizar", "facturar", "todo")]
    [string]$Comando = "stock",

    [Parameter(Mandatory=$false)]
    [ValidateSet("silent", "minimized")]
    [string]$Modo = "silent",

    [Parameter(Mandatory=$false)]
    [string]$LogDir = "$PSScriptRoot\Logs",

    [Parameter(Mandatory=$false)]
    [switch]$EnviarEmail = $false,

    [Parameter(Mandatory=$false)]
    [string]$EmailDestino = "",

    [Parameter(Mandatory=$false)]
    [string]$SMTPServer = "smtp.empresa.com",

    [Parameter(Mandatory=$false)]
    [string]$EmailRemitente = "api-woo@empresa.com"
)

# ===============================================================================
# CONFIGURACIÓN
# ===============================================================================

$AppPath = "$PSScriptRoot\API_FS_WOO.exe"
$Timestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
$FechaLegible = Get-Date -Format "dd/MM/yyyy HH:mm:ss"

# ===============================================================================
# FUNCIONES
# ===============================================================================

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Log {
    param([string]$Message)
    $LogMessage = "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message"
    Write-ColorOutput $LogMessage "Gray"
}

function Ejecutar-Comando {
    param(
        [string]$Comando,
        [string]$LogFile
    )

    Write-Log "Ejecutando comando: $Comando"

    $Arguments = "/$Modo /$Comando"
    if ($LogFile) {
        $Arguments += " /log:`"$LogFile`""
    }

    Write-Log "Argumentos: $Arguments"

    $StartTime = Get-Date

    try {
        $Process = Start-Process -FilePath $AppPath -ArgumentList $Arguments -Wait -PassThru -NoNewWindow

        $EndTime = Get-Date
        $Duration = $EndTime - $StartTime

        if ($Process.ExitCode -eq 0) {
            Write-ColorOutput "✓ Comando '$Comando' completado exitosamente en $($Duration.TotalSeconds) segundos" "Green"
            return $true
        } else {
            Write-ColorOutput "✗ Comando '$Comando' falló con código de salida $($Process.ExitCode)" "Red"
            return $false
        }
    } catch {
        Write-ColorOutput "✗ Error al ejecutar comando: $_" "Red"
        return $false
    }
}

function Enviar-NotificacionEmail {
    param(
        [string]$Asunto,
        [string]$Cuerpo,
        [bool]$Exito
    )

    if (-not $EnviarEmail -or [string]::IsNullOrEmpty($EmailDestino)) {
        return
    }

    Write-Log "Enviando notificación por email a $EmailDestino"

    try {
        $Prioridad = if ($Exito) { "Normal" } else { "High" }

        Send-MailMessage -From $EmailRemitente `
                         -To $EmailDestino `
                         -Subject $Asunto `
                         -Body $Cuerpo `
                         -SmtpServer $SMTPServer `
                         -Priority $Prioridad

        Write-ColorOutput "✓ Email enviado exitosamente" "Green"
    } catch {
        Write-ColorOutput "✗ Error al enviar email: $_" "Yellow"
    }
}

# ===============================================================================
# VALIDACIONES
# ===============================================================================

Write-ColorOutput "`n===============================================================================" "Cyan"
Write-ColorOutput "  API_FS_WOO - Sincronización Automatizada (PowerShell)" "Cyan"
Write-ColorOutput "===============================================================================`n" "Cyan"

# Verificar que existe el ejecutable
if (-not (Test-Path $AppPath)) {
    Write-ColorOutput "ERROR: No se encontró el ejecutable en: $AppPath" "Red"
    exit 1
}

# Crear directorio de logs
if (-not (Test-Path $LogDir)) {
    Write-Log "Creando directorio de logs: $LogDir"
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
}

# ===============================================================================
# INFORMACIÓN DE EJECUCIÓN
# ===============================================================================

Write-ColorOutput "Configuración:" "Yellow"
Write-ColorOutput "  Ejecutable: $AppPath" "White"
Write-ColorOutput "  Comando: $Comando" "White"
Write-ColorOutput "  Modo: $Modo" "White"
Write-ColorOutput "  Logs: $LogDir" "White"
Write-ColorOutput "  Fecha/Hora: $FechaLegible`n" "White"

# ===============================================================================
# EJECUCIÓN
# ===============================================================================

$Resultados = @()
$TodoExitoso = $true
$InicioTotal = Get-Date

if ($Comando -eq "todo") {
    Write-ColorOutput "Ejecutando sincronización completa...`n" "Cyan"

    # 1. Stock
    $LogStock = Join-Path $LogDir "stock_$Timestamp.log"
    $ResultadoStock = Ejecutar-Comando -Comando "stock" -LogFile $LogStock
    $Resultados += @{ Comando = "Stock"; Exito = $ResultadoStock; Log = $LogStock }
    Start-Sleep -Seconds 5

    # 2. Actualizar
    $LogPedidos = Join-Path $LogDir "pedidos_$Timestamp.log"
    $ResultadoPedidos = Ejecutar-Comando -Comando "actualizar" -LogFile $LogPedidos
    $Resultados += @{ Comando = "Pedidos"; Exito = $ResultadoPedidos; Log = $LogPedidos }
    Start-Sleep -Seconds 5

    # 3. Facturar
    $LogFacturas = Join-Path $LogDir "facturas_$Timestamp.log"
    $ResultadoFacturas = Ejecutar-Comando -Comando "facturar" -LogFile $LogFacturas
    $Resultados += @{ Comando = "Facturas"; Exito = $ResultadoFacturas; Log = $LogFacturas }

    $TodoExitoso = $ResultadoStock -and $ResultadoPedidos -and $ResultadoFacturas

} else {
    $LogFile = Join-Path $LogDir "$Comando_$Timestamp.log"
    $ResultadoComando = Ejecutar-Comando -Comando $Comando -LogFile $LogFile
    $Resultados += @{ Comando = $Comando; Exito = $ResultadoComando; Log = $LogFile }
    $TodoExitoso = $ResultadoComando
}

$FinTotal = Get-Date
$DuracionTotal = $FinTotal - $InicioTotal

# ===============================================================================
# RESUMEN
# ===============================================================================

Write-ColorOutput "`n===============================================================================" "Cyan"
Write-ColorOutput "  RESUMEN DE EJECUCIÓN" "Cyan"
Write-ColorOutput "===============================================================================`n" "Cyan"

foreach ($Resultado in $Resultados) {
    $Estado = if ($Resultado.Exito) { "✓ ÉXITO" } else { "✗ FALLO" }
    $Color = if ($Resultado.Exito) { "Green" } else { "Red" }

    Write-ColorOutput "  $($Resultado.Comando): $Estado" $Color
    Write-ColorOutput "    Log: $($Resultado.Log)" "Gray"
}

Write-ColorOutput "`n  Duración total: $([math]::Round($DuracionTotal.TotalMinutes, 2)) minutos" "Yellow"

$EstadoFinal = if ($TodoExitoso) { "COMPLETADO EXITOSAMENTE" } else { "COMPLETADO CON ERRORES" }
$ColorFinal = if ($TodoExitoso) { "Green" } else { "Red" }

Write-ColorOutput "`n  Estado final: $EstadoFinal" $ColorFinal
Write-ColorOutput "`n===============================================================================`n" "Cyan"

# ===============================================================================
# NOTIFICACIÓN POR EMAIL
# ===============================================================================

if ($EnviarEmail -and -not [string]::IsNullOrEmpty($EmailDestino)) {

    $Asunto = if ($TodoExitoso) {
        "✓ API_FS_WOO - Sincronización exitosa ($Comando)"
    } else {
        "✗ API_FS_WOO - Error en sincronización ($Comando)"
    }

    $Cuerpo = @"
Resumen de Sincronización API_FS_WOO
=====================================

Fecha: $FechaLegible
Comando: $Comando
Modo: $Modo
Duración: $([math]::Round($DuracionTotal.TotalMinutes, 2)) minutos
Estado: $EstadoFinal

Resultados:
$(foreach ($Resultado in $Resultados) {
    $Estado = if ($Resultado.Exito) { "✓ ÉXITO" } else { "✗ FALLO" }
    "  $($Resultado.Comando): $Estado`n  Log: $($Resultado.Log)`n"
})

Directorio de Logs: $LogDir

---
Este es un mensaje automático generado por el sistema de sincronización API_FS_WOO.
"@

    Enviar-NotificacionEmail -Asunto $Asunto -Cuerpo $Cuerpo -Exito $TodoExitoso
}

# ===============================================================================
# CÓDIGO DE SALIDA
# ===============================================================================

if ($TodoExitoso) {
    exit 0
} else {
    exit 1
}
