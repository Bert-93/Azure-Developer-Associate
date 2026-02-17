# Arquitectura y despliegue — .NET (API + Web)

![Architecture diagram](../../media/Diagram01.png)

Este directorio contiene una solución .NET con dos proyectos:

- `API` — backend (API REST) situado en la carpeta `API`.
- `Web` — aplicación web frontal situada en la carpeta `Web`.

## Publicar las aplicaciones

Primero inicie sesión en Azure desde la CLI:

```bash
az login
```

A continuación, empaquete cada proyecto en un ZIP (`api.zip` y `web.zip`) y publíquelos usando los comandos siguientes (reemplace los nombres de las apps):

```bash
az webapp deployment source config-zip --resource-group ManagedPlatform --src api.zip --name <name-of-your-api-app>

az webapp deployment source config-zip --resource-group ManagedPlatform --src web.zip --name <name-of-your-web-app>
```

Nota: cree previamente los App Service y el App Service Plan, o despliegue con `az webapp create` si aún no existen.

## Variables de entorno necesarias

- En el App Service del **API** añada una variable de aplicación llamada `StorageConnectionString` con la cadena de conexión a la cuenta de Azure Storage que contendrá las imágenes.
- En el App Service de la **Web** añada una variable de aplicación llamada `ApiUrl` con la URL pública del backend (por ejemplo `https://mi-api.azurewebsites.net/`).

Puede configurar estas variables desde el Portal (Configuration > Application settings) o con la CLI:

```bash
az webapp config appsettings set -g ManagedPlatform -n <name-of-your-api-app> --settings StorageConnectionString="<your-conn-string>"

az webapp config appsettings set -g ManagedPlatform -n <name-of-your-web-app> --settings ApiUrl="https://mi-api.azurewebsites.net/"
```

## Qué hace el `API` (Resumen de `ImagesController.cs`)

- Endpoints:
  - `GET /` — devuelve una lista de URLs (SAS) a las imágenes almacenadas en un contenedor de Blob Storage. Para cada blob genera un SAS con permisos de lectura y expiración corta (5 minutos) y devuelve esas URLs.
  - `POST /` — recibe el cuerpo de la petición como flujo de bytes (imagen) y lo sube a Blob Storage con un nombre generado (GUID sin guiones). Devuelve `201 Created` con la URL del blob.
- Dependencias y configuración:
  - Usa `Options` para leer `StorageConnectionString` y `FullImageContainerName`.
  - Internamente crea el `BlobContainerClient` y llama a `CreateIfNotExistsAsync()` para asegurarse de que el contenedor exista.

En resumen: el API actúa como proxy seguro para almacenar imágenes en Azure Storage y para exponer URIs de acceso temporal (SAS) a esas imágenes.

## Qué hace la `Web` (Resumen de `Index.cshtml` y `Index.cshtml.cs`)

- `Index.cshtml` contiene la interfaz de usuario:
  - Formulario de subida de archivo (`multipart/form-data`) y botón **Upload**.
  - Muestra una galería con las imágenes (cada imagen se renderiza con la URL que proporciona el API).

- `Index.cshtml.cs` (`IndexModel`):
  - En `OnGetAsync()` realiza una petición `GET` a la URL indicada en la variable `ApiUrl` (obtenida desde `Options`) y deserializa la lista de URLs de imágenes para mostrar en la página.
  - En `OnPostAsync()` toma el fichero subido (`Upload`) y lo envía al API mediante `HttpClient.PostAsync(imagesUrl, image)` usando `StreamContent` con el `Content-Type` correcto.

En conjunto: la Web es una UI ligera que consume el API para listar y subir imágenes.

## Buenas prácticas y siguientes pasos

- Use `Managed Identity` y `Key Vault` si quiere evitar exponer cadenas de conexión en Application Settings directamente.
- Marque como `Slot setting` las variables que no deba intercambiar entre ranuras (si usa deployment slots).
- Considere habilitar Application Insights en ambos App Services para monitorización y alertas.
- Si desea, puedo añadir ejemplos de `GitHub Actions` que construyan los proyectos .NET y generen los `api.zip`/`web.zip` automáticamente.
