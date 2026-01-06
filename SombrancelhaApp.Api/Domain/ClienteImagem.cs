namespace SombrancelhaApp.Api.Domain;

public class ClienteImagem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ClienteId { get; private set; }
    public string Caminho { get; private set; }
    public DateTime CriadoEm { get; private set; }

    protected ClienteImagem() { }

    public ClienteImagem(Guid clienteId, string caminho)
    {
        ClienteId = clienteId;
        Caminho = caminho;
        CriadoEm = DateTime.UtcNow;
    }
}
