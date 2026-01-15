using SombrancelhaApp.Api.Domain;

namespace SombrancelhaApp.Api.Repositories;

public interface IClienteRepository
{
    // Métodos de Escrita (Assíncronos)
    Task AddAsync(Cliente cliente);
    Task UpdateAsync(Cliente cliente);
    Task DeleteAsync(Cliente cliente);

    // Métodos de Leitura
    Task<Cliente?> GetByIdAsync(Guid id);
    
    // Este é o método que permite o filtro por nome no Swagger
    Task<(IEnumerable<Cliente> Items, int TotalCount)> GetPagedAsync(string? nome, int page, int pageSize);

    IQueryable<Cliente> Query();
}