# Funciones .NET — Detalle de Http2 y Http3

En la carpeta `FUNC` hay un pequeño proyecto de Azure Functions con dos funciones HTTP llamadas `Http2` y `Http3`.

## Qué hacen ambas funciones

- `Http2` y `Http3` reciben peticiones HTTP (`GET` y `POST`) que pueden incluir un parámetro `nombre` en la query string o en el cuerpo JSON.
- Ambas crean un mensaje de respuesta (por ejemplo: "{nombre}, función procesada.") y registran una entrada en una tabla llamada `operaciones` para trazar la invocación.

## Diferencia principal: binding vs acceso manual a la Tabla

- `Http2` utiliza el binding de salida de Azure Functions. En el código se ve el atributo:

```csharp
[TableOutput("operaciones", Connection = "AzureWebJobsStorage2")]
public ITableEntity Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
{
    // ... crea y devuelve un ITableEntity (Registro) que Functions inserta en la tabla
}
```

  - Comportamiento: la función construye un objeto `Registro` (implementando `ITableEntity`) y lo devuelve; el runtime de Functions se encarga de escribir ese objeto en la tabla `operaciones` usando la conexión indicada por la variable de entorno `AzureWebJobsStorage2`.

- `Http3` realiza la inserción de forma manual desde el código:

```csharp
string storageConnection = Environment.GetEnvironmentVariable("AzureWebJobsStorage2");
var storageClient = new TableServiceClient(storageConnection);
var table = storageClient.GetTableClient("operaciones");
var registro = new Registro() { Nombre = nombre, Mensaje = mensaje, PartitionKey = "http3" };
table.AddEntity(registro);
```

  - Comportamiento: la función obtiene la cadena de conexión desde la variable de entorno `AzureWebJobsStorage2`, crea un `TableServiceClient` y llama a `AddEntity(...)` para insertar el registro explícitamente.

## Datos que se registran

- Ambos registros usan la misma tabla `operaciones`.
- En `Http2` el `PartitionKey` se establece a `http2`.
- En `Http3` el `PartitionKey` se establece a `http3`.

## Requisitos de configuración

- Configure la cadena de conexión de Azure Storage en la aplicación de funciones (Configuration / Application settings) con la clave `AzureWebJobsStorage2` y el valor del connection string de su Storage Account.

```bash
az functionapp config appsettings set -g ManagedPlatform -n <your-functionapp-name> --settings AzureWebJobsStorage2="<connection-string>"
```

## Comandos para crear las funciones (desde local)

Si parte de cero, puede crear el proyecto y las funciones con Azure Functions Core Tools:

```bash
# Crear el proyecto (en la carpeta .NET)
func init . --worker-runtime dotnet

# Crear las funciones HTTP (plantilla Http Trigger)
func new --name Http2 --template "Http Trigger" --authlevel function
func new --name Http3 --template "Http Trigger" --authlevel function
```

Después, reemplace el contenido generado por los ficheros `Http2.cs` y `Http3.cs` que ya existen en este repositorio (o adapte el código para incluir el binding de `TableOutput` en `Http2`).

## Comandos para publicar las funciones

Opción A — Publicar con `func` (recomendado para desarrollo / despliegue sencillo):

```bash
func azure functionapp publish <your-functionapp-name>
```

Opción B — Publicar mediante ZIP y `az` (útil en CI):

```bash
# Compilar y publicar localmente a una carpeta
dotnet publish -c Release -o publish

# Crear un zip con el contenido publicado
cd publish
zip -r ../function.zip .

# Desplegar por zip
az functionapp deployment source config-zip --resource-group ManagedPlatform --name <your-functionapp-name> --src ../function.zip
```

Recuerde establecer la variable `AzureWebJobsStorage2` en la configuración del `Function App` antes de ejecutar las funciones en Azure.

Si quieres, puedo añadir aquí fragmentos concretos de `func` y del `function.json` del binding de tabla para `Http2`, o crear un pequeño workflow de GitHub Actions que compile y despliegue las funciones automáticamente.
