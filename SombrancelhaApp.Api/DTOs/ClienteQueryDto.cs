namespace SombrancelhaApp.Api.DTOs;

public class ClienteQueryDto
{
    public string? Nome { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? OrderBy { get; set; }
}
