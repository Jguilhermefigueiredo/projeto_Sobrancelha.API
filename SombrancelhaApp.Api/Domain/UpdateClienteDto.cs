namespace SombrancelhaApp.Api.DTOs;

public class UpdateClienteDto
{
    public string Nome { get; set; } = null!;
    public int Idade { get; set; }
    public string Telefone { get; set; } = null!;
}
