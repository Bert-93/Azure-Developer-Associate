# Azure Blob Storage — Guía rápida

Este documento resume conceptos clave y comandos para trabajar con **Azure Blob Storage**: explorar el servicio, administrar el ciclo de vida de los blobs y realizar operaciones comunes.

## 1. Exploración de Azure Blob Storage

- ¿Qué es: Azure Blob Storage es un servicio de almacenamiento de objetos para datos no estructurados (imágenes, vídeos, backups, logs, archivos estáticos).
- Dónde y cómo explorarlo:
  - **Azure Portal**: inspeccionar cuentas de almacenamiento, contenedores, configuraciones y reglas de ciclo de vida.
  - **Azure Storage Explorer** (aplicación de escritorio): explorar contenedores, descargar/subir blobs y gestionar SAS tokens.
  - **Azure CLI**: listar cuentas, contenedores y blobs.
    - Ejemplo: `az storage container list --account-name <acct>`
    - Ejemplo: `az storage blob list --account-name <acct> -c <container>`
  - **AzCopy**: herramienta de línea de comandos optimizada para mover grandes volúmenes de datos.
  - **SDKs** (.NET, Python, Java, JavaScript): para integración programática y operaciones avanzadas.
- Características importantes:
  - Tipos de blob: Block blobs (archivos grandes), Append blobs (logs) y Page blobs (discos VHD).
  - Niveles de acceso: Hot, Cool y Archive (optimización de coste según frecuencia de acceso).
  - Seguridad: cifrado en reposo (por defecto), SAS tokens y Managed Identities.

## 2. Administración del ciclo de vida de Azure Blob Storage

- Objetivo: minimizar costes y gestionar retención y borrado automático de datos.
- Herramientas y conceptos:
  - **Access tiers**: mover blobs entre Hot/Cool/Archive para optimizar costes.
  - **Management Policy (Lifecycle rules)**: reglas que, por condiciones (edad, prefijo, blob index tags), realizan acciones: moveToCool, moveToArchive, delete.
    - Se configura desde el Portal o con `az storage account management-policy create --account-name <acct> --resource-group <rg> --policy @policy.json`.
  - **Versioning** y **Soft Delete**: habilítelos para proteger contra borrados accidentales y para retener versiones históricas.
  - **Immutability policies / WORM**: cuando necesites cumplir normativas (no permitir borrado/modificación durante un periodo).
  - **Snapshots**: copia punto-in-time de un blob (útil para backups rápidos).
- Buenas prácticas:
  - Defina políticas de ciclo de vida coherentes con SLA y requisitos de archivado.
  - Use archiving para datos fríos y configure retenciones mínimas cuando sea necesario para cumplimiento.
  - Habilite soft delete y versioning para recuperación rápida ante borrados accidentales.

## 3. Trabajo con Azure Blob Storage (operaciones comunes)

- Crear cuenta de Storage y contenedor (CLI):

```bash
az storage account create -n mystorageacct -g MyResourceGroup -l westus2 --sku Standard_LRS
az storage container create --account-name mystorageacct -n mycontainer
```

- Subir y descargar blobs (CLI):

```bash
# Subir un archivo
az storage blob upload --account-name mystorageacct -c mycontainer -f ./path/to/file.jpg -n file.jpg

# Descargar un blob
az storage blob download --account-name mystorageacct -c mycontainer -n file.jpg --file ./file.jpg
```

- Uso de AzCopy (rápido, recomendado para grandes volúmenes):

```bash
# Copiar local -> blob
azcopy copy "./data/*" "https://<account>.blob.core.windows.net/mycontainer?sas_token" --recursive
```

- Generar SAS (Shared Access Signature) para acceso temporal:

```bash
# Generar SAS con permisos de lectura por 1 hora (ejemplo con Azure CLI v2 preview de storage):
az storage blob generate-sas --account-name mystorageacct --container-name mycontainer --name file.jpg --permissions r --expiry 2026-12-31T23:59:00Z --auth-mode login
```

- Acceso desde código (ejemplo conceptual .NET):

```csharp
var client = new BlobServiceClient(new DefaultAzureCredential());
var container = client.GetBlobContainerClient("mycontainer");
await container.CreateIfNotExistsAsync();
await container.UploadBlobAsync("file.jpg", File.OpenRead("./file.jpg"));
```

- Seguridad y prácticas recomendadas:
  - Use **Managed Identity** en lugar de connection strings cuando se ejecute desde recursos de Azure (App Service, Functions, VM).
  - Use SAS con permisos mínimos y expiración corta para exponer blobs públicamente.
  - Configure CORS si su contenido será consumido desde navegadores.
  - Active **logging**, **metrics** y **diagnostics** para monitorizar accesos y rendimiento.
  - Considere usar **Azure CDN** para servir blobs estáticos a escala global y mejorar latencias.

## Recursos y comandos útiles

- Documentación: https://learn.microsoft.com/azure/storage/blobs
- AzCopy: https://learn.microsoft.com/azure/storage/common/storage-use-azcopy-v10
- Azure Storage Explorer: https://learn.microsoft.com/azure/storage/common/storage-explorer
