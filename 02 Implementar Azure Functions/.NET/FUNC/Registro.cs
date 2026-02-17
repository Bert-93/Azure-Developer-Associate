using Azure.Data.Tables;
using Azure;

namespace DemoCurso;

public class Registro : ITableEntity
{
    public string Nombre { get; set; } = "";
    public string Mensaje { get; set; } = "";

    public string PartitionKey { get; set; } = "HTTP";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}