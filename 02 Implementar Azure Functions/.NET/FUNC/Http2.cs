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
    public class Http2
    {
        private readonly ILogger<Http2> _logger;

        public Http2(ILogger<Http2> logger)
        {
            _logger = logger;
        }

        [Function("Http2")]
        [TableOutput("operaciones", Connection = "AzureWebJobsStorage2")]
        public ITableEntity Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP2 trigger function processed a request.");

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

            // Respuesta HTTP
            req.HttpContext.Response.Headers.ContentType = "text/plain; charset=UTF-8";
            req.HttpContext.Response.StatusCode = 200;
            req.HttpContext.Response.WriteAsync(mensaje);

            // Registro para la tabla
            var registro = new Registro()
            {
                Nombre = nombre,
                Mensaje = mensaje,
                PartitionKey = "http2"
            };

            return registro;
        }
    }
}
