# Proyecto COSMOS (.NET)

Este proyecto es una pequeña aplicación de consola en C# que demuestra operaciones CRUD básicas sobre Azure Cosmos DB (API SQL). El código de ejemplo se encuentra en `COSMOS/Program.cs` y utiliza el SDK de .NET para Cosmos DB.

## Objetivo

Mostrar cómo realizar las operaciones principales sobre documentos en un contenedor de Cosmos DB:
- Crear (Insert)
- Leer (Read) — uno o varios documentos, con y sin filtros
- Actualizar (Update / Upsert)
- Borrar (Delete)

También se destacan patrones importantes como uso de `PartitionKey`, consultas con `QueryDefinition` y paginación con `FeedIterator`.

## Estructura relevante

- `COSMOS/Program.cs`: implementación del menú y los métodos que realizan las operaciones sobre Cosmos DB.

## Sentencias C# y su operativa en la base de datos

Los métodos del proyecto usan directamente las llamadas del SDK de Cosmos. A continuación se explican las sentencias y patrones más importantes.

- Inicializar cliente y contenedor

```csharp
CosmosClient client = new CosmosClient(endpoint, key);
Container container = client.GetContainer(databaseId, containerId);
```

Descripción: crea una instancia reusable de `CosmosClient` (recomendado mantenerla como singleton) y obtiene una referencia al `Container` donde se almacenan los documentos.

- Insertar (Create)

```csharp
await container.CreateItemAsync(cliente, new PartitionKey(cliente.direccion.ciudad));
```

Descripción: inserta un nuevo documento. El segundo parámetro indica la `PartitionKey` usada por Cosmos DB; aquí se usa la ciudad del cliente. Si ya existe un documento con la misma id y partición, esta llamada fallará.

- Leer un documento por id y partición (Read)

```csharp
var response = await container.ReadItemAsync<Cliente>(id, new PartitionKey(ciudad));
```

Descripción: lectura directa por identificador y clave de partición. Esta es la operación más eficiente cuando conoces ambas claves porque es una lectura dirigida a una partición concreta.

- Consultar varios documentos / filtrar (Query)

```csharp
var query = new QueryDefinition("SELECT * FROM c WHERE c.direccion.ciudad = @ciudad")
    .WithParameter("@ciudad", "Madrid");
using FeedIterator<Cliente> iterator = container.GetItemQueryIterator<Cliente>(query);
while (iterator.HasMoreResults)
{
    foreach (Cliente item in await iterator.ReadNextAsync())
    {
        // procesar item
    }
}
```

Descripción: cuando necesitas recuperar varios documentos o aplicar filtros, se construye una `QueryDefinition` y se itera con `FeedIterator<T>` que gestiona la paginación (`ReadNextAsync()` devuelve páginas - `FeedResponse<T>`).

- Actualizar (Upsert / Replace)

```csharp
await container.UpsertItemAsync(cliente, new PartitionKey(cliente.direccion.ciudad));
// o ReplaceItemAsync si se quiere forzar reemplazo
```

Descripción: `UpsertItemAsync` inserta o actualiza el documento según exista o no; es conveniente cuando no se desea comprobar previamente la existencia. `ReplaceItemAsync` reemplaza un documento existente (requiere id y partición).

- Borrar (Delete)

```csharp
await container.DeleteItemAsync<Cliente>(cliente.id, new PartitionKey(cliente.direccion.ciudad));
```

Descripción: elimina un documento identificándolo por `id` y `PartitionKey`.

## Consideraciones y métodos adicionales de interés

- `CreateDatabaseIfNotExistsAsync` y `CreateContainerIfNotExistsAsync`: útiles para preparar el entorno (crear BD/contenedor si no existen).

- `QueryRequestOptions`: permite optimizar consultas (por ejemplo `MaxItemCount`, `PartitionKey` precisa al ejecutar la consulta, o `MaxConcurrency`).

- Manejo de excepciones y throttling (`429`): el SDK puede devolver respuestas `RequestRateTooLarge`; implemente reintentos respetando el `Retry-After` y/o use un policy de reintento.

- Diagnósticos: capture `response.Diagnostics` y registre `ActivityId`/latencias si observa problemas de rendimiento.

- Uso de async/await: todas las operaciones del SDK son asíncronas; usar `async` mejora escalabilidad y throughput.

- Reutilizar `CosmosClient`: crear múltiples instancias suele ser ineficiente y puede agotar recursos; mantenga el cliente como singleton.

## Ejecución rápida

1. Configure `endpoint` y `key` en `COSMOS/Program.cs`.
2. Compile y ejecute la aplicación de consola.
3. Use el menú para probar Insertar, Leer, Modificar, Borrar y Listar.

## Recursos

- Documentación oficial del SDK de Azure Cosmos DB para .NET: https://learn.microsoft.com/azure/cosmos-db/
