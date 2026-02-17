# Azure Cosmos DB — Guía rápida

Este documento resume conceptos y ejemplos prácticos para **explorar** Azure Cosmos DB, **trabajar con C#** y ejemplos de **server-side**: Stored Procedures, Triggers y UDFs.

## 1. Exploración de Azure Cosmos DB

- ¿Qué es: Azure Cosmos DB es un servicio de base de datos multimodelo, distribuido globalmente, con baja latencia y escalado transparente.
- Dónde explorarlo:
  - **Azure Portal**: crear cuentas, revisar métricas, ver throughput (RU/s), configurar replicación global y ver contenedores/ítems.
  - **Azure CLI**: `az cosmosdb` para crear cuentas y `az cosmosdb sql` para bases/contenedores.
  - **Data Explorer** en el Portal o la extensión de Cosmos DB en VS Code para consultar datos y probar stored procs/triggers/UDFs.
  - **Emulador**: use el Azure Cosmos DB Emulator para desarrollo local.
  
### Ventajas principales de Azure Cosmos DB

- **Replicación global**: replica datos en múltiples regiones Azure con un solo clic, proporcionando alta disponibilidad y redundancia geográfica.
- **Baja latencia**: diseñado para ofrecer latencias de lectura y escritura a un solo dígito de milisegundos en el edge mediante réplicas locales.
- **Escalabilidad**: escala el throughput (RU/s) y el almacenamiento de forma elástica; soporta escalado manual y autoscale.
- **Niveles de coherencia configurables**: ofrece varias opciones de coherencia para equilibrar rendimiento y consistencia (vea abajo).


### RU (Request Unit) y modelos de facturación

- **¿Qué es una RU?**: una Request Unit (RU) es la unidad lógica de consumo en Cosmos DB que abstrae el coste de operación (lectura, escritura, consulta, índices). Cada operación consume un número de RUs dependiendo de su complejidad y tamaño.
- **Modelos de facturación**:
  - **Provisioned Throughput (RU/s)**: usted aprovisiona una cantidad fija de RU/s para un contenedor o base de datos. Se factura por las RU/s provisionadas (y por almacenamiento). Puede elegir aprovisionamiento manual o usar **Autoscale** para ajustar automáticamente entre un mínimo y un máximo.
  - **Autoscale**: variante del provisioned throughput donde Cosmos escala automáticamente las RU/s en función de la carga hasta un máximo configurado. Se factura en función de la RU/s máxima utilizada en cada periodo y del consumo real.
  - **Serverless (consumption)**: modelo por demanda donde no se aprovisionan RU/s. Se factura únicamente por las RUs consumidas por las operaciones y el almacenamiento; indicado para cargas esporádicas o bajas.
- **Notas**: además del coste por RU, se factura el almacenamiento y otras capacidades (replicación entre regiones, backups, networking). El throughput puede aprovisionarse a nivel de contenedor o a nivel de base de datos (compartido entre contenedores).

- Modelos y APIs: SQL (Core), MongoDB, Cassandra, Gremlin (graph) y Table. El modelo SQL/Core es el más frecuente para documentos JSON y server-side JS.

### Niveles de coherencia

Azure Cosmos DB proporciona cinco niveles de coherencia, ordenados de más a menos estrictos:

- **Strong**: lecturas siempre ven la escritura más reciente en toda la cuenta (consistencia global fuerte). Requiere sincronización y puede incrementar latencias entre regiones.
- **Bounded Staleness**: garantiza que las lecturas no estén más atrasadas que un número máximo de versiones u operaciones (K) o un período de tiempo (t); útil cuando se tolera cierto retraso controlado.
- **Session**: coherencia por sesión; el cliente siempre ve sus propias escrituras en una sesión. Es la opción por defecto para muchas aplicaciones cliente-servidor.
- **Consistent Prefix**: lecturas reflejan un prefijo consistente de escrituras (no se verá orden inverso), pero pueden estar retrasadas.
- **Eventual**: la opción más débil; eventual convergencia sin garantías de orden ni frescura, pero con máxima disponibilidad y menor latencia.

La elección del nivel de coherencia afecta latencia, disponibilidad y coste (RU consumption).

### Jerarquía de recursos en Cosmos DB

La estructura principal de Cosmos DB es:

- **Cuenta (Account)**: el contenedor de mayor nivel que agrupa configuraciones globales, claves, endpoints y regiones replicadas.
- **Base de datos (Database)**: agrupación lógica de contenedores; puede crear varias bases dentro de una cuenta.
- **Contenedor (Container) / Colección**: almacena ítems/documents; definido por un `partition key` y configuraciones de throughput (RU/s) por contenedor o compartido a nivel de base.
- **Items / Documents**: los objetos JSON que contienen los datos reales; cada item tiene un `id` y valores que determinan su `partitionKey`.

### APIs admitidas y detalle

- **SQL (Core)**: API nativa para documentos JSON, con consulta SQL-like; soporta server-side JavaScript (stored procs, triggers, UDFs).
- **MongoDB**: interfaz compatible con el protocolo de MongoDB, permitiendo usar drivers y herramientas de MongoDB sobre Cosmos DB.
- **Cassandra**: compatible con el protocolo Apache Cassandra; útil para migraciones de tablas y workloads de Cassandra.
- **Gremlin (graph)**: diseñado para grafos y consultas de travesía de grafos usando Gremlin.
- **Table**: compatible con Azure Table Storage API para escenarios de key-value/simple NoSQL.

Cada API tiene características y limitaciones particulares; por ejemplo, las stored procedures y triggers sólo existen en la API SQL/Core porque se ejecutan como JavaScript dentro del motor de Cosmos DB.

## 2. Trabajo con C# en Azure Cosmos DB

- SDK: utilice el paquete `Microsoft.Azure.Cosmos` (SDK v3/v4). Recomendación: rehúse crear clientes repetidamente; use una instancia singleton de `CosmosClient`.
- Ejemplo básico (C#) — crear cliente, base y contenedor, upsert y consulta:

```csharp
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

var endpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT");
var key = Environment.GetEnvironmentVariable("COSMOS_KEY");
var client = new CosmosClient(endpoint, key);

var database = await client.CreateDatabaseIfNotExistsAsync("MyDatabase");
var container = await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
{
    Id = "Items",
    PartitionKeyPath = "/partitionKey"
}, throughput: 400);

// Upsert (insert or replace)
var item = new { id = Guid.NewGuid().ToString(), partitionKey = "p1", name = "contoso" };
await container.Container.UpsertItemAsync(item, new PartitionKey(item.partitionKey));

// Query
var sql = "SELECT c.name FROM c WHERE c.partitionKey = @pk";
var query = container.Container.GetItemQueryIterator<dynamic>(new QueryDefinition(sql).WithParameter("@pk","p1"));
while (query.HasMoreResults)
{
    foreach (var r in await query.ReadNextAsync()) Console.WriteLine(r.name);
}
```

- Buenas prácticas:
  - Reutilice `CosmosClient` como singleton.
  - Maneje `RequestRateTooLargeException` (429) con backoff exponencial.
  - Modele particiones con alta cardinalidad y que reflejen patrones de consulta.
  - Monitoree RU/s, latencia y throttling; ajuste throughput o use autoscale.

## 3. Ejemplos server-side: Stored Procedures, Triggers y UDFs

Nota: las stored procedures, triggers y UDFs se escriben en JavaScript y se ejecutan en el motor de Cosmos DB (área de un único partition key para stored procs/transactions).

- Stored Procedure (ejemplo: insertar múltiples ítems en una transacción):

```js
function bulkUpsert(items) {
  var context = getContext();
  var container = context.getCollection();
  var response = context.getResponse();

  var created = 0;
  if (!items) throw new Error('items is undefined');

  var callback = function(err, item, options) {
    if (err) throw err;
    created++;
    if (created >= items.length) response.setBody(created);
  };

  for (var i = 0; i < items.length; i++) {
    var isAccepted = container.createDocument(container.getSelfLink(), items[i], callback);
    if (!isAccepted) throw new Error('Request not accepted by server.');
  }
}
```

- Trigger (ejemplo `pre` trigger) — añadir timestamp antes de insertar un documento:

```js
function addTimestamp() {
  var context = getContext();
  var request = context.getRequest();
  var doc = request.getBody();
  doc._createdOn = new Date().toISOString();
  request.setBody(doc);
}
```

- UDF (User Defined Function) — concatenar nombre y apellido:

```js
function fullName(first, last) {
  return first + ' ' + last;
}
```

Uso y consideraciones:
  - Las stored procedures y triggers se ejecutan en el mismo **partition key**; para operaciones transaccionales asegúrese de que los ítems compartan la misma partición.
  - Las UDFs se pueden usar en consultas SQL: `SELECT udf.fullName(c.first, c.last) FROM c`.
  - Para procesamiento escalable y near-real-time prefiera **Change Feed** + Azure Functions en lugar de triggers server-side.

## 4. Comandos útiles (CLI)

- Crear cuenta de Cosmos DB (ejemplo SQL API):

```bash
az cosmosdb create -n my-cosmos-account -g MyResourceGroup --kind GlobalDocumentDB

# Crear base de datos y contenedor
az cosmosdb sql database create --account-name my-cosmos-account -g MyResourceGroup -n MyDatabase
az cosmosdb sql container create --account-name my-cosmos-account -g MyResourceGroup --database-name MyDatabase -n Items --partition-key-path "/partitionKey" --throughput 400
```

## 5. Recursos y siguientes pasos

- Documentación: https://learn.microsoft.com/azure/cosmos-db
- SDK C#: https://learn.microsoft.com/azure/cosmos-db/sql/how-to-use-dotnet
- Server-side programming (stored procs/triggers/UDFs): https://learn.microsoft.com/azure/cosmos-db/programming

Si quieres, puedo:
- Añadir ejemplos de código C# más detallados (gestión de errores, retry, bulk/TransactionalBatch).
- Crear ejemplos JSON/JS ya cargables desde Data Explorer (stored procedure, trigger, UDF).
