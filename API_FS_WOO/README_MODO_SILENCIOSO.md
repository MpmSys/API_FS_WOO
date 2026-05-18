# API_FS_WOO - Modo Silencioso

## ✅ Implementado

La aplicación ahora soporta **ejecución en modo silencioso** (sin interfaz gráfica) para automatizar tareas.

---

## 🚀 Uso Rápido

### Modo Silencioso (Sin ventana)
```batch
API_FS_WOO.exe /silent /stock
API_FS_WOO.exe /silent /actualizar
API_FS_WOO.exe /silent /facturar
```

### Modo Minimizado (Ventana minimizada)
```batch
API_FS_WOO.exe /minimized /stock
API_FS_WOO.exe /minimized /actualizar
API_FS_WOO.exe /minimized /facturar
```

### Con Log
```batch
API_FS_WOO.exe /silent /stock /log:"C:\Logs\stock.txt"
```

---

## 📋 Comandos Disponibles

| Comando | Descripción |
|---------|-------------|
| `/stock` | Descarga el stock desde FacturaScripts |
| `/actualizar` | Actualiza los pedidos desde WooCommerce |
| `/facturar` | Factura los pedidos pendientes |

---

## 🛠️ Scripts Incluidos

### `Ejecutar_Tareas.bat`
Script batch interactivo con menú para ejecutar tareas.

**Uso:**
```batch
Ejecutar_Tareas.bat
```

### `Ejecutar-API.ps1`
Script PowerShell avanzado con soporte para notificaciones por email.

**Ejemplos:**
```powershell
# Descargar stock
.\Ejecutar-API.ps1 -Comando stock

# Todo (stock + pedidos + facturas)
.\Ejecutar-API.ps1 -Comando todo

# Con notificación por email
.\Ejecutar-API.ps1 -Comando todo -EnviarEmail -EmailDestino "admin@empresa.com"
```

---

## 📅 Programar Tareas en Windows

1. Abrir **Programador de Tareas**
2. Crear tarea básica
3. Configurar:
   - **Programa:** `C:\Ruta\API_FS_WOO.exe`
   - **Argumentos:** `/silent /stock /log:"C:\Logs\stock.txt"`
4. ✅ Listo

---

## 📖 Documentación Completa

Ver archivo `MODO_SILENCIOSO.md` para documentación detallada con ejemplos y configuración del Programador de Tareas.

---

## ✨ Características

- ✅ Ejecución sin interfaz gráfica (`/silent`)
- ✅ Ejecución minimizada (`/minimized`)
- ✅ Generación de logs (`/log:"ruta"`)
- ✅ Compatible con versión anterior (`stock`, `actualizar`)
- ✅ Ideal para tareas programadas
- ✅ Scripts batch y PowerShell incluidos
- ✅ Soporte para notificaciones por email (PowerShell)

---

## 🔧 Configuración

La aplicación utiliza la configuración existente en:
```
%USERPROFILE%\Documents\MpmSystems\BaseApi.cfg
```

No requiere configuración adicional para el modo silencioso.
