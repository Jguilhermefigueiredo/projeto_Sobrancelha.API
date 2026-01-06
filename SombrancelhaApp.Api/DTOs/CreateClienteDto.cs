using System.ComponentModel.DataAnnotations;

namespace SombrancelhaApp.Api.DTOs;

public class CreateClienteDto
{
    [Required]
    [MinLength(3)]
    public string Nome { get; set; } = string.Empty;

    [Range(1, 120)]
    public int Idade { get; set; }

    [Required]
    [Phone]
    public string Telefone { get; set; } = string.Empty;
}
