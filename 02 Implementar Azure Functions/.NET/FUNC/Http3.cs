using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;

namespace DemoCurso
{
    public class Http3
    {
        private readonly ILogger<Http2> _logger;

        public Http3(ILogger<Http2> logger)
        {
            _logger = logger;
        }

        [Function("Http3")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP3 trigger function processed a request.");

            // Parámetro de la URL
            string nombre = req.Query["nombre"]!;

            // Datos en el cuerpo del mensaje JSON
            string cuerpoJSON = new StreamReader(req.Body)
                .ReadToEndAsync()
                .Result;

            dynamic cuerpoOBJ = JsonConvert.DeserializeObject(cuerpoJSON)!;

            // Asignación de datos
            nombre = nombre ?? cuerpoOBJ?.nombre!;

            // Mensaje
            string mensaje = string.IsNullOrEmpty(nombre)
                ? "Procesado sin nombre."
                : $"{nombre}, función procesada.";


            string storageConnection = Environment.GetEnvironmentVariable("AzureWebJobsStorage2")!;
            if (!string.IsNullOrEmpty(storageConnection))
            {
                // Conexión con la tabla
                var storageClient = new TableServiceClient(storageConnection);
                var table = storageClient.GetTableClient("operaciones");

                // Registro para la tabla
                var registro = new Registro()
                {
                    Nombre = nombre,
                    Mensaje = mensaje,
                    PartitionKey = "http3"
                };

                // Añadir el registro a la tabla
                table.AddEntity(registro);
            }                

            // Respuesta HTTP
            return new OkObjectResult(mensaje);
        }
    }
}
