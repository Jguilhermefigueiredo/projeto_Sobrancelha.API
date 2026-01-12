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

    // Liberação de cache / Controle de Ciclo de Vida
    public bool ConfirmadoParaDeletar { get; set; } = false;
    public bool AvisadoSobreExpiracao { get; set; } = false;

    // Relacionamento: Quem fez o trabalho
    public Guid UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!; // Propriedade de navegação

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}