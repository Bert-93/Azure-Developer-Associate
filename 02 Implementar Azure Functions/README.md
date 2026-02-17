# Azure Functions — Guía rápida

Este documento resume conceptos y pasos básicos para **explorar** y **desarrollar** con Azure Functions.

## 1. Exploración de Azure Functions

- ¿Qué es: Azure Functions es la plataforma serverless de Azure para ejecutar código en respuesta a eventos (triggers) con modelo de facturación por consumo o en planes dedicados.
- Triggers y Bindings:
  - Triggers comunes: `HttpTrigger`, `TimerTrigger`, `BlobTrigger`, `QueueTrigger`, `ServiceBusTrigger`.
  - Bindings simplifican entrada/salida a servicios (Blob, Queue, Table, Cosmos DB, etc.) sin código de infraestructura.
- Modelos de ejecución y planes:
  - **Consumption Plan**: escala automáticamente y factura por ejecuciones y duración.
  - **Premium Plan**: instancias siempre calientes, escalado más rápido y VNet Integration.
  - **App Service Plan**: ejecuta funciones en máquinas dedicadas.
- Lenguajes soportados: C# (.NET), JavaScript/TypeScript, Python, Java, PowerShell y contenedores personalizados.
- Herramientas para explorar:
  - Azure Portal: crear, probar y ver invocaciones.
  - Azure CLI: `az functionapp` para gestionar recursos.
  - Azure Functions Core Tools (`func`) para desarrollo local.
  - VS Code + extensión Azure Functions para crear y depurar funciones.
- Observabilidad y seguridad:
  - Habilitar Application Insights para trazas, métricas y análisis de rendimiento.
  - Usar Managed Identity para acceder a secretos y recursos sin credenciales embebidas.

## 2. Desarrollo de Azure Functions

- Inicio rápido (local):

```bash
# Instalar Functions Core Tools (si no está instalado)
# Windows (Chocolatey): choco install azure-functions-core-tools-4

# Crear proyecto .NET
func init MyFunctionApp --worker-runtime dotnet

# Crear nueva función HTTP
cd MyFunctionApp
func new --name HttpExample --template "Http Trigger" --authlevel anonymous

# Ejecutar localmente
func start
```

- Ejemplos de comandos de despliegue y creación de recursos:

```bash
# Crear cuenta de Storage (requerida por Function App)
az storage account create -n mystorageacct -g MyResourceGroup -l westus2 --sku Standard_LRS

# Crear Function App (Consumption plan)
az functionapp create --resource-group MyResourceGroup --consumption-plan-location westus2 --name my-func-app --storage-account mystorageacct --runtime dotnet

# Publicar desde el proyecto local
func azure functionapp publish my-func-app
```

- Configuración y secretos:
  - Use `az functionapp config appsettings set` o la sección Configuration del Portal para añadir variables de aplicación y cadenas de conexión.
  - Prefiera Key Vault + Managed Identity para secretos sensibles.

- Desarrollo y depuración:
  - Depure localmente con VS Code (attach o launch configurado) o use `func start` y puntos de interrupción.
  - Use Azurite o el emulador de Storage para pruebas locales de bindings que dependen de Storage.

- Buenas prácticas:
  - Mantenga funciones pequeñas y con una sola responsabilidad.
  - Maneje correctamente límites de ejecución y tiempos de espera (cold start, timeouts).
  - Configure retrys y manejo de errores para triggers basados en colas/mensajería.
  - Monitorice latencias y coste con Application Insights y alertas.

## Recursos

- Documentación oficial: https://learn.microsoft.com/azure/azure-functions
- Azure Functions Core Tools: https://learn.microsoft.com/azure/azure-functions/functions-run-local
- Ejemplos y plantillas: extensión Azure Functions para VS Code
