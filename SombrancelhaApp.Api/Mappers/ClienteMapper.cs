using SombrancelhaApp.Api.Domain;
using SombrancelhaApp.Api.DTOs;

namespace SombrancelhaApp.Api.Mappers;

public static class ClienteMapper
{
    public static ClienteResponseDto ToDto(this Cliente cliente)
    {
        return new ClienteResponseDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Idade = cliente.Idade,
            Telefone = cliente.Telefone,
            CriadoEm = cliente.CriadoEm
        };
    }
}
