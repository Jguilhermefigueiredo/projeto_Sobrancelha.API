namespace SombrancelhaApp.Api.Domain;

public class AtendimentoSimulacao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClienteId { get; set; } = string.Empty;
    public string NomeMolde { get; set; } = string.Empty;
    public string CorHex { get; set; } = string.Empty;

    // Armazenamos o caminho físico para manutenção e a URL para o Front-end
    public string CaminhoImagemFinal { get; set; } = string.Empty;
    public string UrlImagemFinal { get; set; } = string.Empty;

    //liberação de cache
    public bool ConfirmadoParaDeletar { get; set; } = false;
    public bool AvisadoSobreExpiracao { get; set; } = false; // para notificações

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}