namespace SombrancelhaApp.Api.DTOs; // Namespace ajustado para sua pasta na raiz

public class SimulacaoRespostaDto
{
    public string ClienteId { get; set; } = string.Empty;
    public string UrlImagemFinal { get; set; } = string.Empty;
    public DateTime DataProcessamento { get; set; }
}