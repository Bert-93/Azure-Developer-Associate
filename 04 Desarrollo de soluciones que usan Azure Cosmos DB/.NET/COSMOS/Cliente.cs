namespace CosmosClientApp;


public class Cliente
{
    public string id { get; set; }
    public string companyName { get; set; }
    public Contacto contact { get; set; }
    public Direccion direccion { get; set; }
    public string telefono { get; set; }
    public bool activo { get; set; }
}
