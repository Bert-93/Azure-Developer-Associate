# Azure Contenedores — Guía rápida

## Administración de imágenes de contenedor en Azure Container Registry (ACR)

### Características y ventajas
- Registro privado y gestionado para imágenes de contenedor.
- Integración con Azure AD para control de acceso y roles.
- Replicación geográfica y soporte para redes privadas (VNet).
- Integración con Azure Security Center y escaneo de vulnerabilidades (defender).
- Soporta OCI, Helm charts y artefactos de build.

### ACR Tasks: automatización de compilaciones e implementaciones
ACR Tasks permite automatizar compilaciones, reconstrucciones y despliegues directamente en el registro.

- Flujos Git-based: compilar cuando hay cambios en un repositorio remoto.
- Triggers de base image para reconstruir imágenes cuando cambia una imagen base.
- Soporta tareas en paralelo y build en servidor (sin necesidad de Docker local).

Ejemplo: crear una tarea que compile desde un repositorio Git

```bash
az acr task create \
	--registry MyRegistry \
	--name build-from-git \
	--context https://github.com/mi-org/mi-repo.git#main \
	--file Dockerfile \
	--image myapp:{{.Run.ID}} \
	--commit-trigger-enabled true
```

### Elementos de un Dockerfile
- FROM: imagen base.
- WORKDIR: directorio de trabajo dentro del contenedor.
- COPY / ADD: copiar archivos desde el contexto de build.
- RUN: ejecutar comandos durante la construcción (instalación de paquetes).
- ENV: definir variables de entorno en tiempo de build/runtime.
- EXPOSE: puerto que expone la imagen (documental).
- CMD / ENTRYPOINT: comando por defecto al iniciar el contenedor.

Buenas prácticas:
- Minimizar capas combinando instrucciones `RUN` cuando sea posible.
- Usar imágenes base oficiales y ligeras.
- Usar `.dockerignore` para reducir el contexto de build.

### Compilación y ejecución de una imagen en ACR con la CLI de Azure

Construir y subir con `az acr build` (build en la nube, sin Docker local):

```bash
az acr build --registry MyRegistry --image myapp:1.0 .
```

Alternativa con Docker local (login, tag y push):

```bash
docker build -t myapp:1.0 .
az acr login --name MyRegistry
docker tag myapp:1.0 myregistry.azurecr.io/myapp:1.0
docker push myregistry.azurecr.io/myapp:1.0
```

## Ejecución de imágenes de contenedor en Azure Container Instances (ACI)

### Ventajas de Azure Container Instances
- Ejecución rápida y sin gestión de VMs: provisioning instantáneo de contenedores.
- Pago por segundo de ejecución y recursos asignados.
- Ideal para cargas efímeras, jobs y prototipos.
- Se pueden agrupar recursos en un grupo de contenedores (container group) que comparte red y almacenamiento.

### Implementar una instancia de contenedor con la CLI

```bash
az container create \
	--resource-group MyResourceGroup \
	--name mycontainer \
	--image myregistry.azurecr.io/myapp:1.0 \
	--registry-login-server myregistry.azurecr.io \
	--registry-username <usuario> \
	--registry-password <password> \
	--cpu 1 --memory 1.5 \
	--dns-name-label myapp-label \
	--ports 80
```

### Iniciar y detener contenedores mediante directivas
ACI soporta la propiedad `--restart-policy` para controlar el comportamiento tras fallos o finalización:

- `Always`: reinicia siempre (por defecto en muchos escenarios de long-running).
- `OnFailure`: reinicia solo si el proceso finaliza con error.
- `Never`: no reinicia.

Además, se pueden iniciar y detener manualmente con:

```bash
az container stop --resource-group MyResourceGroup --name mycontainer
az container start --resource-group MyResourceGroup --name mycontainer
```

### Establecer variables de entorno

```bash
az container create \
	--resource-group MyResourceGroup \
	--name mycontainer \
	--image myregistry.azurecr.io/myapp:1.0 \
	--environment-variables ENV1=valor1 ENV2=valor2
```

### Montar recursos compartidos de archivos (Azure File Share)

```bash
az container create \
	--resource-group MyResourceGroup \
	--name mycontainer \
	--image myregistry.azurecr.io/myapp:1.0 \
	--azure-file-volume-share-name myshare \
	--azure-file-volume-account-name mystorageacct \
	--azure-file-volume-account-key <key> \
	--azure-file-volume-mount-path /mnt/azfiles
```

## Implementación de Azure Container Apps

### Ventajas y características principales
- Plataforma serverless para contenedores: escala automática (incluso a cero) usando KEDA.
- Revisions y despliegues sin downtime; routing por revision y tráfico.
- Integración con Dapr para building blocks (pub/sub, state, bindings).
- Ingress integrado con TLS y dominio personalizado.
- Entorno gestionado (Container Apps Environment) que agrupa apps y comparte redes.
- Soporte para secretos, configuración y conexiones a registros privados.

Ejemplo de despliegue básico con CLI:

```bash
# crear el environment (una sola vez)
az containerapp env create --name myEnv --resource-group MyResourceGroup --location eastus

az containerapp create \
	--name myapp \
	--resource-group MyResourceGroup \
	--environment myEnv \
	--image myregistry.azurecr.io/myapp:latest \
	--ingress external \
	--target-port 80 \
	--registry-server myregistry.azurecr.io \
	--registry-username <user> \
	--registry-password <password>
```
