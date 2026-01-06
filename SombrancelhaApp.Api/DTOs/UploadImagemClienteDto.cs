using Microsoft.AspNetCore.Http;

namespace SombrancelhaApp.Api.DTOs;

public class UploadImagemClienteDto
{
    public IFormFile Imagem { get; set; } = null!;
}
