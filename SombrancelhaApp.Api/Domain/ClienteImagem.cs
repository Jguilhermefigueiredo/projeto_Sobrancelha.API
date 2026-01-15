namespace SombrancelhaApp.Api.Domain;

public class ClienteImagem
{

    public ClienteImagem()
    {
        Caminho = string.Empty;
    }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClienteId { get; set; }
    public string Caminho { get; set; }
    public DateTime CriadoEm { get; set; }



    public ClienteImagem(Guid clienteId, string caminho)
    {
        ClienteId = clienteId;
        Caminho = caminho;
        CriadoEm = DateTime.UtcNow;
    }
}
