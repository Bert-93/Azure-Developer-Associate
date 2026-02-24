
# Docker — Comandos prácticos y Dockerfile ejemplo

Este documento contiene comandos `docker` comentados para tareas comunes (descargar, listar, borrar imágenes; crear, parar, arrancar y eliminar contenedores; acceder a la shell; copiar ficheros; crear una imagen con commit; etiquetar y enviar a un registro de Azure). Se usan las imágenes `nginx:latest` y `nginx:alpine` como ejemplos.

## Imágenes: descargar, listar y borrar

```bash
# Descargar (pull) la imagen desde Docker Hub
docker pull nginx:latest

# Listar imágenes locales
docker images

# Borrar una imagen local (forzar si está en uso dará error)
docker rmi nginx:latest

# Borrar la imagen alpine
docker pull nginx:alpine
docker rmi nginx:alpine
```

## Crear contenedores y mapear puertos

```bash
# Crear y ejecutar un contenedor a partir de nginx:latest, mapeando el puerto 8080 del host al 80 del contenedor
docker run -d --name mi-nginx -p 8080:80 nginx:latest

# Variante usando nginx:alpine (imagen más ligera)
docker run -d --name mi-nginx-alpine -p 8081:80 nginx:alpine

# Crear con reinicio automático al arrancar el host
docker run -d --name mi-nginx --restart unless-stopped -p 8080:80 nginx:latest
```

## Parar, arrancar y eliminar contenedores

```bash
# Parar un contenedor
docker stop mi-nginx

# Arrancar un contenedor parado
docker start mi-nginx

# Reiniciar
docker restart mi-nginx

# Eliminar un contenedor (debe estar detenido)
docker rm mi-nginx

# Forzar eliminación (parar si está en ejecución y eliminar)
docker rm -f mi-nginx
```

## Entrar en la shell de un contenedor y ejecutar comandos

```bash
# Abrir una shell interactiva dentro del contenedor (si tiene /bin/sh)
docker exec -it mi-nginx /bin/sh

# Ejecutar un comando puntual dentro del contenedor sin abrir shell interactiva
docker exec mi-nginx nginx -v
```

## Copiar ficheros hacia/desde el contenedor

```bash
# Copiar archivo local -> contenedor
docker cp ./mi-index.html mi-nginx:/usr/share/nginx/html/index.html

# Copiar archivo contenedor -> local
docker cp mi-nginx:/etc/nginx/nginx.conf ./nginx.conf
```

## Crear una nueva imagen desde un contenedor (`commit`)

```bash
# Hacer cambios en un contenedor y crear una imagen nueva basada en su estado
docker commit mi-nginx mi-nginx-custom:1.0

# Ver la nueva imagen
docker images | grep mi-nginx-custom
```

## Etiquetar y enviar una imagen a Azure Container Registry (ACR)

```bash
# Supongamos que tu registro ACR es myregistry.azurecr.io
# Etiquetar la imagen local con el nombre del registro
docker tag mi-nginx-custom:1.0 myregistry.azurecr.io/mi-nginx-custom:1.0

# Iniciar sesión en ACR (opción 1: con az acr login)
az acr login --name myregistry

# O con credenciales Docker (si prefieres docker login)
docker login myregistry.azurecr.io --username <usuario> --password <password>

# Enviar la imagen al registro
docker push myregistry.azurecr.io/mi-nginx-custom:1.0
```

## Dockerfile típico para una aplicación web ASP.NET Core MVC (comentado)

```dockerfile
# Stage 1: build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Directorio de trabajo dentro del contenedor
WORKDIR /src

# Copiar el csproj y restaurar dependencias (cacheable)
COPY ["MiApp/MiApp.csproj", "MiApp/"]
RUN dotnet restore "MiApp/MiApp.csproj"

# Copiar el resto del código y publicar
COPY . .
WORKDIR /src/MiApp
RUN dotnet publish "MiApp.csproj" -c Release -o /app/publish

# Stage 2: runtime (imagen ligera con sólo runtime)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

# Directorio donde se copiará la publicación
WORKDIR /app

# Copiar desde la etapa de build
COPY --from=build /app/publish ./

# Exponer puerto que usa la aplicación (documental)
EXPOSE 80

# El comando por defecto para ejecutar la app
ENTRYPOINT ["dotnet", "MiApp.dll"]
```

Notas sobre el Dockerfile:
- Es multi-stage para reducir el tamaño final de la imagen (se construye con el SDK y se ejecuta con el runtime).
- Ajusta la versión de .NET (`6.0`) según tu proyecto.

## Comandos para construir, etiquetar y subir la imagen .NET a ACR

```bash
# Desde el directorio raíz del repo que contiene la carpeta MiApp

# Construir imagen local con docker build
docker build -t miapp:1.0 -f MiApp/Dockerfile .

# Etiquetar para ACR (reemplaza myregistry por tu registro)
docker tag miapp:1.0 myregistry.azurecr.io/miapp:1.0

# Iniciar sesión en ACR (usando la CLI de Azure)
az acr login --name myregistry

# Enviar la imagen al registro
docker push myregistry.azurecr.io/miapp:1.0
```
