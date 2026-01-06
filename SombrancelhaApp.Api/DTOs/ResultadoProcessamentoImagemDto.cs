namespace SombrancelhaApp.Api.DTOs;

public class ResultadoProcessamentoImagemDto
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public string? CaminhoImagemNormalizada { get; set; }
}
