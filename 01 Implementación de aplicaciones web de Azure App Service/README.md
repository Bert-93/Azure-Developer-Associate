# Azure App Service — Guía rápida

Este documento resume conceptos y prácticas clave para trabajar con **Azure App Service**: explorar la plataforma, configurar aplicaciones web, escalar aplicaciones y usar ranuras de implementación.

## 1. Explorar Azure App Service

- ¿Qué es: Azure App Service es un servicio PaaS para hospedar aplicaciones web, APIs y backends móviles. Soporta Windows, Linux y contenedores.
- Dónde explorarlo: Azure Portal, Azure CLI (`az webapp`), Azure PowerShell (`New-AzWebApp`), y la extensión de App Service en VS Code.
- Características destacadas:
  - Integración con CI/CD (GitHub Actions, Azure DevOps, FTP, ZIP deploy).
  - Soporte de stacks (Node, .NET, Python, Java, PHP) y contenedores personalizados.
  - Monitorización integrada (Application Insights, métricas, logs de diagnóstico).
  - Autenticación/Autorización administrada (Easy Auth) y Managed Identity.
  - Dominios personalizados, certificados TLS y balanceo de carga integrado.

## 2. Configurar aplicaciones web

- Creación: puede crearse desde el Portal o con la CLI: `az webapp create --resource-group <rg> --plan <plan> --name <app-name> --runtime "NODE|14-lts"`.
- Ajustes y configuración:
  - **Application settings**: variables de entorno y cadenas de conexión desde el Portal o `az webapp config appsettings set`.
  - **Startup / Command**: para contenedores o stacks específicos, configure el comando de inicio.
  - **Identity**: habilite Managed Identity para acceder a otros recursos de Azure sin credenciales en código.
  - **Networking**: VNet Integration, Private Endpoints y Access Restrictions para controlar acceso.
  - **Diagnósticos**: habilite logs de servidor HTTP, logs de aplicaciones y streaming logs para depuración.
- Despliegue:
  - Usar pipelines (GitHub Actions / Azure DevOps) para despliegues automatizados.
  - Para despliegues manuales: `az webapp deployment source config-zip -g <rg> -n <app> --src app.zip`.

## 3. Escalado de aplicaciones en Azure App Service

- Tipos de escalado:
  - **Scale up (vertical)**: cambiar el Plan de App Service a un SKU superior (más CPU/memoria, características).
  - **Scale out (horizontal)**: aumentar el número de instancias en el App Service Plan.
- Modos de escalado:
  - **Manual**: ajustar manualmente el número de instancias en el Portal o CLI.
  - **Autoscale**: reglas basadas en métricas (CPU, memoria, Http Queue, etc.) o programadas (horarios pico).
- Consideraciones:
  - El escalado se aplica al App Service Plan (varias apps pueden compartir el mismo plan).
  - Para cargas muy altas y aislamiento, considere App Service Environments (ASE) o distribuir cargas entre regiones.
  - Pruebe reglas de autoscale en preproducción y configure alertas cuando se activen.

## 4. Exploración de ranuras de implementación (Deployment Slots)

- ¿Qué son?: Las ranuras son instancias secundarias (staging, testing, etc.) de la misma app que permiten validar cambios antes de enviarlos a producción.
- Flujo típico:
  1. Desplegar a la ranura `staging`.
  2. Probar la app en la ranura pública o con tráfico parcial.
  3. Realizar `swap` para intercambiar `staging` y `production` sin tiempo de inactividad.
- Ventajas:
  - Permiten pruebas en un entorno lo más cercano posible a producción.
  - Los swaps realizan warm-up y minimizan downtime.
- Configuración y buenas prácticas:
  - Cree ranuras desde el Portal o CLI: `az webapp deployment slot create -g <rg> -n <app> --slot staging`.
  - Use **slot settings** (marcar variables como “Slot setting”) para que ciertas configuraciones no se intercambien durante el swap (por ejemplo, cadenas de conexión de pruebas vs producción).
  - Antes de hacer swap, prepare scripts de warm-up y compruebe dependencias externas.
  - Use tráfico progresivo (routing) para enrutamiento gradual a la ranura si necesita canary releases.
  - Evite almacenar secretos en ranuras no seguras; use Key Vault y Managed Identity.

## Recursos y siguientes pasos

- Documentación oficial: https://learn.microsoft.com/azure/app-service
- Comandos CLI útiles:
  - `az webapp create`, `az webapp config appsettings set`, `az webapp deployment source config-zip`
  - `az webapp deployment slot create`, `az webapp deployment slot swap`
