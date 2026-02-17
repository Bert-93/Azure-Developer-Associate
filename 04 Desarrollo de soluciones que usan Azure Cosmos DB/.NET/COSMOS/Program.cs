using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using Microsoft.Azure.Cosmos;

namespace CosmosClientApp;

class Program
{
    static string endpoint = "https://cosmosdbdemosbcr.documents.azure.com:443/";
    static string key = "ZGMgcZJKTrvp0ol8xtMjbicSmH95Do0Z24SiLttFmiU2xD4RRDZq2ZZER1cO2YIhaQyhi3jtXCGhACDb7PW0Dg==";
    static string databaseId = "northwind";
    static string containerId = "customers";

    /// <summary>
    /// Punto de entrada de la aplicación de consola.
    /// Inicializa el cliente de Cosmos DB, obtiene el contenedor y muestra
    /// un menú interactivo para operaciones CRUD sobre clientes.
    /// </summary>
    /// <param name="args">Argumentos de línea de comandos (no usados).</param>
    static async Task Main(string[] args)
    {
        // Establecer el cliente para la conexión y el contenedor
        CosmosClient client = new CosmosClient(endpoint, key);
        Container container = client.GetContainer(databaseId, containerId);

        while (true)
        {
            Console.WriteLine("\n===== MENU COSMOS CLIENTES =====");
            Console.WriteLine("1 - Insertar Cliente");
            Console.WriteLine("2 - Leer Cliente");
            Console.WriteLine("3 - Modificar Cliente");
            Console.WriteLine("4 - Borrar Cliente");
            Console.WriteLine("5 - Listar Clientes");
            Console.WriteLine("0 - Salir");

            var opcion = Console.ReadLine();
            Console.Clear();

            switch (opcion)
            {
                case "1":
                    await InsertarCliente(container);
                    break;
                case "2":
                    await LeerCliente(container);
                    break;
                case "3":
                    await ActualizarCliente(container);
                    break;
                case "4":
                    await BorrarCliente(container);
                    break;
                case "5":
                    await ListarClientes(container);
                    break;
                case "0":
                    return;
            }       
        } 
    }

    /// <summary>
    /// Solicita datos por consola y crea un nuevo documento de tipo <see cref="Cliente"/>
    /// en el contenedor especificado. Usa la ciudad como clave de partición.
    /// </summary>
    /// <param name="container">Instancia del contenedor de Cosmos DB donde insertar.</param>
    static async Task InsertarCliente(Container container)
    {
        Console.Clear();
        Console.Write("ID: ");
        string id = Console.ReadLine();

        Console.Write("Empresa: ");
        string empresa = Console.ReadLine();

        Console.Write("Contacto: ");
        string contacto = Console.ReadLine();

        Console.Write("Ciudad: ");
        string ciudad = Console.ReadLine();

        Console.Write("País: ");
        string pais = Console.ReadLine();         

        // Creación del objeto que representa al cliente
        Cliente cliente = new Cliente()
        {            
            id = id,
            companyName = empresa,
            contact = new Contacto() { contactName = contacto },
            direccion = new Direccion() { ciudad = ciudad, pais = pais },
            telefono = "600700800",
            activo = true
        };

        // Insertamos el registro en CosmosDB
        try
        {
            await container.CreateItemAsync(cliente, new PartitionKey(cliente.direccion.ciudad));
            Console.WriteLine("Registro insertado correctamente.");
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }

    }

    /// <summary>
    /// Lee y muestra los datos de un cliente por su <c>id</c> y <c>ciudad</c> (clave de partición).
    /// Muestra un mensaje si el cliente no existe.
    /// </summary>
    /// <param name="container">Contenedor de Cosmos DB del que leer el documento.</param>
    static async Task LeerCliente(Container container)
    {
        Console.Write("ID Cliente: ");
        string id = Console.ReadLine();

        Console.Write("Ciudad: ");
        string ciudad = Console.ReadLine();

        try
        {
            var response = await container.ReadItemAsync<Cliente>(id, new PartitionKey(ciudad));
            Console.WriteLine($"Empresa: {response.Resource.companyName}");
        }
        catch
        {
            Console.WriteLine("Cliente no encontrado");
        }
    }

    /// <summary>
    /// Recupera un cliente existente, solicita campos a actualizar por consola
    /// y realiza un <c>Upsert</c> para guardar los cambios en Cosmos DB.
    /// </summary>
    /// <param name="container">Contenedor de Cosmos DB donde está el documento.</param>
    static async Task ActualizarCliente(Container container)
    {
        Console.Clear();
        Console.Write("ID Cliente: ");
        string id = Console.ReadLine();

        Console.Write("Ciudad: ");
        string ciudad = Console.ReadLine();

        try
        {
            var respuesta = await container.ReadItemAsync<Cliente>(id, new PartitionKey(ciudad));
            Console.WriteLine($"Cliente: {respuesta.Resource.companyName}");

            Cliente cliente = respuesta.Resource;

            Console.Write("Contacto: ");
            cliente.contact.contactName = Console.ReadLine();

            Console.Write("Cargo: ");
            cliente.contact.contactTitle = Console.ReadLine();
            
            Console.Write("Calle: ");
            cliente.direccion.calle = Console.ReadLine();     

            await container.UpsertItemAsync(cliente, new PartitionKey(cliente.direccion.ciudad));
            Console.WriteLine("Registro modificado correctamente.");     
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    /// <summary>
    /// Recupera un cliente por <c>id</c> y <c>ciudad</c>, pide confirmación
    /// al usuario y, si se confirma, elimina el documento del contenedor.
    /// </summary>
    /// <param name="container">Contenedor de Cosmos DB desde el que borrar.</param>
    static async Task BorrarCliente(Container container)
    {
        Console.Clear();
        Console.Write("ID Cliente: ");
        string id = Console.ReadLine();

        Console.Write("Ciudad: ");
        string ciudad = Console.ReadLine();

        try
        {
            var respuesta = await container.ReadItemAsync<Cliente>(id, new PartitionKey(ciudad));
            Console.WriteLine($"Cliente: {respuesta.Resource.companyName}");
            
            Console.Write("¿Desea borrar el cliente (S/N)? ");
            string question = Console.ReadLine().ToLower();
            
            if(question == "si" || question == "s")
            {
                Cliente cliente = respuesta.Resource;
                
                await container.DeleteItemAsync<Cliente>(cliente.id, new PartitionKey(cliente.direccion.ciudad));
                Console.WriteLine("Registro eliminado correctamente.");    
            } 
            else Console.WriteLine("Sin cambios en el registro.");
        }
        catch (System.Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    /// <summary>
    /// Consulta y lista todos los documentos de tipo <see cref="Cliente"/>
    /// presentes en el contenedor usando una consulta simple "SELECT * FROM c".
    /// </summary>
    /// <param name="container">Contenedor de Cosmos DB a consultar.</param>
    static async Task ListarClientes(Container container)
    {
        // Recuperar todos los documentos según la SELECT
        var query = new QueryDefinition("SELECT * FROM c");

        using FeedIterator<Cliente> iterator = container.GetItemQueryIterator<Cliente>(query);

        Console.Clear();
        while (iterator.HasMoreResults)
        {
            foreach (Cliente item in await iterator.ReadNextAsync())
            {
                Console.WriteLine($"{item.id}# {item.companyName} - {item.direccion.ciudad} ({item.direccion.pais})");
            }
        }
    }

}