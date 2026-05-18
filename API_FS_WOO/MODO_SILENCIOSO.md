# Modo Silencioso - API_FS_WOO

## Descripción
La aplicación API_FS_WOO ahora soporta ejecución en modo silencioso (sin interfaz gráfica) y minimizado, ideal para automatizar tareas con el Programador de Tareas de Windows.

---

## Sintaxis

```
API_FS_WOO.exe [modo] [comando] [opciones]
```

---

## Modos de Ejecución

### `/silent` - Modo Silencioso
- **No muestra ninguna ventana**
- Ideal para tareas programadas desatendidas
- Cierra automáticamente al terminar
- No aparece en la barra de tareas

### `/minimized` - Modo Minimizado
- Muestra la ventana minimizada en la barra de tareas
- Permite ver el progreso si se restaura la ventana
- No cierra automáticamente

---

## Comandos Disponibles

| Comando | Descripción |
|---------|-------------|
| `/stock` | Descarga el stock desde FacturaScripts |
| `/actualizar` | Actualiza los pedidos desde WooCommerce |
| `/facturar` | Factura los pedidos pendientes |

---

## Opciones Adicionales

### `/log:"ruta_archivo"`
Guarda un log detallado de la ejecución en el archivo especificado.

**Ejemplo:**
```
API_FS_WOO.exe /silent /stock /log:"C:\Logs\API_FS_WOO\stock.log"
```

---

## Ejemplos de Uso

### 1. Descargar Stock (Modo Silencioso)
```batch
API_FS_WOO.exe /silent /stock
```

### 2. Actualizar Pedidos (Modo Silencioso con Log)
```batch
API_FS_WOO.exe /silent /actualizar /log:"C:\Logs\pedidos_%date%.txt"
```

### 3. Facturar Pedidos (Modo Minimizado)
```batch
API_FS_WOO.exe /minimized /facturar
```

### 4. Múltiples Opciones
```batch
API_FS_WOO.exe /silent /stock /log:"D:\MpmSystems\Logs\stock.txt"
```

---

## Compatibilidad con Versión Anterior

La aplicación sigue siendo compatible con los argumentos antiguos:

```batch
API_FS_WOO.exe stock        (Equivalente a /minimized /stock)
API_FS_WOO.exe actualizar   (Equivalente a /minimized /actualizar)
```

---

## Programación de Tareas en Windows

### Ejemplo: Tarea para descargar stock diariamente

1. **Abrir el Programador de Tareas de Windows**
   - Presiona `Win + R` → Escribe `taskschd.msc` → Enter

2. **Crear Tarea Básica**
   - Nombre: `API_FS_WOO - Descargar Stock Diario`
   - Descripción: `Descarga automática de stock desde FacturaScripts`

3. **Configurar Desencadenador**
   - Frecuencia: Diariamente
   - Hora: `02:00 AM`
   - Repetir: Cada día

4. **Configurar Acción**
   - Acción: `Iniciar un programa`
   - Programa: `C:\Ruta\A\API_FS_WOO.exe`
   - Argumentos: `/silent /stock /log:"C:\Logs\stock_%date%.txt"`
   - Iniciar en: `C:\Ruta\A\`

5. **Configuraciones Adicionales**
   - ✅ Ejecutar aunque el usuario no haya iniciado sesión
   - ✅ Ejecutar con los privilegios más altos
   - ✅ Oculta (solo si es necesario)

---

## Script Batch de Ejemplo

Crea un archivo `.bat` para ejecutar múltiples tareas:

### `actualizar_todo.bat`
```batch
@echo off
echo ================================================
echo    API_FS_WOO - Actualizacion Automatica
echo ================================================
echo.

set LOGDIR=C:\Logs\API_FS_WOO
set APPPATH=C:\Program Files\MpmSystems\API_FS_WOO.exe
set FECHA=%date:~-4%-%date:~3,2%-%date:~0,2%

echo [%time%] Creando directorio de logs...
if not exist "%LOGDIR%" mkdir "%LOGDIR%"

echo [%time%] Descargando stock...
"%APPPATH%" /silent /stock /log:"%LOGDIR%\stock_%FECHA%.log"

echo [%time%] Esperando 10 segundos...
timeout /t 10 /nobreak > nul

echo [%time%] Actualizando pedidos...
"%APPPATH%" /silent /actualizar /log:"%LOGDIR%\pedidos_%FECHA%.log"

echo [%time%] Esperando 10 segundos...
timeout /t 10 /nobreak > nul

echo [%time%] Facturando pedidos...
"%APPPATH%" /silent /facturar /log:"%LOGDIR%\facturas_%FECHA%.log"

echo.
echo ================================================
echo    Proceso completado exitosamente
echo ================================================
pause
```

---

## PowerShell Script de Ejemplo

### `Actualizar-API.ps1`
```powershell
# Script de PowerShell para automatizar API_FS_WOO
param(
    [string]$Comando = "stock",
    [string]$LogDir = "C:\Logs\API_FS_WOO"
)

$AppPath = "C:\Program Files\MpmSystems\API_FS_WOO.exe"
$Timestamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
$LogFile = Join-Path $LogDir "$Comando_$Timestamp.log"

# Crear directorio de logs si no existe
if (-not (Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
}

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  API_FS_WOO - Ejecución Automatizada" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Comando: $Comando" -ForegroundColor Yellow
Write-Host "Log: $LogFile" -ForegroundColor Yellow
Write-Host ""

# Ejecutar aplicación
$Arguments = "/silent /$Comando /log:`"$LogFile`""
Write-Host "Ejecutando: $AppPath $Arguments" -ForegroundColor Green
Start-Process -FilePath $AppPath -ArgumentList $Arguments -Wait

Write-Host ""
Write-Host "Proceso completado. Revisa el log en:" -ForegroundColor Green
Write-Host $LogFile -ForegroundColor White
```

**Uso:**
```powershell
# Descargar stock
.\Actualizar-API.ps1 -Comando stock

# Actualizar pedidos
.\Actualizar-API.ps1 -Comando actualizar -LogDir "D:\Logs"

# Facturar
.\Actualizar-API.ps1 -Comando facturar
```

---

## Códigos de Salida

| Código | Descripción |
|--------|-------------|
| `0` | Éxito |
| `1` | Error general |
| `-1` | Comando no reconocido |

---

## Logs

Los logs incluyen:
- ✅ Fecha y hora de inicio/fin
- ✅ Comando ejecutado
- ✅ Base de datos utilizada
- ✅ Resultados de operaciones
- ✅ Errores y advertencias

**Ejemplo de log:**
```
[2025-01-24 02:00:01] Iniciando en modo silencioso...
Comando: stock
Base de datos: C:\Users\Usuario\Documents\MpmSystems
Ejecutando: Descarga de stock...
Descargando stock de FacturaScripts...
Total de variantes procesadas: 156
[2025-01-24 02:02:35] Finalizando aplicación en modo silencioso...
```

---

## Notas Importantes

1. **Permisos**: Asegúrate de que el usuario que ejecuta la tarea tenga permisos de escritura en:
   - La base de datos Access
   - El directorio de logs (si se especifica)

2. **Base de Datos**: La aplicación busca automáticamente la configuración en:
   ```
   %USERPROFILE%\Documents\MpmSystems\BaseApi.cfg
   ```

3. **Timeout**: Las operaciones asíncronas tienen un tiempo de espera de 2 segundos antes de cerrar en modo silencioso.

4. **Debug**: Todos los mensajes se escriben en la salida de Debug de Visual Studio para facilitar la depuración.

---

## Solución de Problemas

### La aplicación no se cierra automáticamente
- Verifica que estés usando `/silent` (no `/minimized`)
- Revisa que no haya errores en el log

### No se crea el archivo de log
- Verifica que la ruta existe y tienes permisos de escritura
- Usa comillas dobles si la ruta contiene espacios

### La tarea programada no se ejecuta
- Verifica que la ruta del ejecutable es correcta
- Asegúrate de que el usuario tiene permisos
- Revisa el historial del Programador de Tareas

---

## Soporte

Para más información o reportar problemas, contacta al equipo de MpmSystems.
