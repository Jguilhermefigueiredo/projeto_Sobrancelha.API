namespace SombrancelhaApp.Api.DTOs;

public class ClienteResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
